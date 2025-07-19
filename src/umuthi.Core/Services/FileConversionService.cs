using Microsoft.Extensions.Logging;
using System.Text;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Presentation;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace umuthi.Core.Services;

/// <summary>
/// Service implementation for converting files to text and extracting content
/// </summary>
public class FileConversionService : IFileConversionService
{
    private static readonly string FileBoundary = "=" + new string('=', 80);
    private static readonly string FileEndBoundary = new string('-', 80);

    public async Task<FileConversionResponse> ConvertFilesToTextAsync(FileConversionRequest request, ILogger logger)
    {
        var startTime = DateTime.UtcNow;
        logger.LogInformation("Starting file conversion for {FileCount} files", request.Files.Length);

        var response = new FileConversionResponse
        {
            TotalFileCount = request.Files.Length
        };

        var processedFiles = new List<ProcessedFileResult>();

        // Process each file
        for (int i = 0; i < request.Files.Length; i++)
        {
            var fileInput = request.Files[i];
            
            try
            {
                logger.LogInformation("Processing file {Index}: {FileName} ({MimeType})", 
                    i + 1, fileInput.FileName, fileInput.MimeType);

                var result = await ProcessSingleFileAsync(fileInput, logger);
                processedFiles.Add(result);

                if (result.Success)
                {
                    response.ProcessedFileCount++;
                    logger.LogInformation("Successfully processed file {Index}: {CharCount} characters extracted", 
                        i + 1, result.TextContent.Length);
                }
                else
                {
                    response.ProcessingErrors.Add($"{fileInput.FileName}: {result.ErrorMessage}");
                    logger.LogWarning("Failed to process file {Index}: {Error}", i + 1, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error processing file {Index}: {FileName}", i + 1, fileInput.FileName);
                
                var errorResult = new ProcessedFileResult
                {
                    FileName = fileInput.FileName,
                    Success = false,
                    ErrorMessage = ex.Message,
                    FileType = SupportedFileTypes.GetFileTypeName(fileInput.MimeType)
                };
                processedFiles.Add(errorResult);
                response.ProcessingErrors.Add($"{fileInput.FileName}: {ex.Message}");
            }
        }

        // Format the results
        response.FormattedText = FormatProcessedFiles(processedFiles);
        response.TotalCharacterCount = response.FormattedText.Length;
        response.ProcessingDurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

        logger.LogInformation("File conversion completed. Processed {ProcessedCount}/{TotalCount} files. Total characters: {CharCount}",
            response.ProcessedFileCount, response.TotalFileCount, response.TotalCharacterCount);

        return response;
    }

    public async Task<ProcessedFileResult> ProcessSingleFileAsync(FileInput fileInput, ILogger logger)
    {
        var result = new ProcessedFileResult
        {
            FileName = fileInput.FileName,
            FileType = SupportedFileTypes.GetFileTypeName(fileInput.MimeType)
        };

        try
        {
            if (!IsMimeTypeSupported(fileInput.MimeType))
            {
                result.Success = false;
                result.ErrorMessage = $"Unsupported file type: {fileInput.MimeType}";
                return result;
            }

            var binaryData = Convert.FromBase64String(fileInput.BinaryData);
            
            result.TextContent = fileInput.MimeType.ToLowerInvariant() switch
            {
                SupportedFileTypes.Pdf => await ExtractTextFromPdf(binaryData, logger),
                SupportedFileTypes.WordDocx => await ExtractTextFromWordDocx(binaryData, logger),
                SupportedFileTypes.WordDoc => await ExtractTextFromWordDoc(binaryData, logger),
                SupportedFileTypes.ExcelXlsx => await ExtractTextFromExcelXlsx(binaryData, logger),
                SupportedFileTypes.ExcelXls => await ExtractTextFromExcelXls(binaryData, logger),
                SupportedFileTypes.PowerPointPptx => await ExtractTextFromPowerPointPptx(binaryData, logger),
                SupportedFileTypes.PowerPointPpt => await ExtractTextFromPowerPointPpt(binaryData, logger),
                SupportedFileTypes.PlainText => await ExtractTextFromPlainText(binaryData, logger),
                SupportedFileTypes.RichText => await ExtractTextFromRichText(binaryData, logger),
                _ => throw new NotSupportedException($"MIME type {fileInput.MimeType} is not supported")
            };

            result.Success = true;
            logger.LogDebug("Extracted {CharCount} characters from {FileName}", 
                result.TextContent.Length, fileInput.FileName);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            logger.LogError(ex, "Error extracting text from file {FileName} ({MimeType})", 
                fileInput.FileName, fileInput.MimeType);
        }

        return result;
    }

    public string FormatProcessedFiles(List<ProcessedFileResult> processedFiles)
    {
        var formatted = new StringBuilder();
        
        formatted.AppendLine("FILE CONVERSION RESULTS");
        formatted.AppendLine(FileBoundary);
        formatted.AppendLine($"Processed {processedFiles.Count(f => f.Success)} of {processedFiles.Count} files");
        formatted.AppendLine($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        formatted.AppendLine(FileBoundary);
        formatted.AppendLine();

        for (int i = 0; i < processedFiles.Count; i++)
        {
            var file = processedFiles[i];
            
            formatted.AppendLine($"FILE {i + 1}: {file.FileName}");
            formatted.AppendLine($"Type: {file.FileType}");
            formatted.AppendLine($"Status: {(file.Success ? "✓ SUCCESS" : "✗ FAILED")}");
            
            if (file.Success)
            {
                formatted.AppendLine($"Content Length: {file.TextContent.Length:N0} characters");
                formatted.AppendLine(FileEndBoundary);
                formatted.AppendLine();
                
                if (!string.IsNullOrWhiteSpace(file.TextContent))
                {
                    formatted.AppendLine(file.TextContent.Trim());
                }
                else
                {
                    formatted.AppendLine("[No text content found in this file]");
                }
            }
            else
            {
                formatted.AppendLine($"Error: {file.ErrorMessage}");
                formatted.AppendLine(FileEndBoundary);
                formatted.AppendLine();
                formatted.AppendLine("[File could not be processed]");
            }
            
            formatted.AppendLine();
            formatted.AppendLine(FileBoundary);
            formatted.AppendLine();
        }

        return formatted.ToString();
    }

    public bool IsMimeTypeSupported(string mimeType)
    {
        return SupportedFileTypes.IsSupported(mimeType);
    }

    #region Private Text Extraction Methods

    private async Task<string> ExtractTextFromPdf(byte[] data, ILogger logger)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var document = PdfDocument.Open(data);
                var text = new StringBuilder();

                for (int i = 1; i <= document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i);
                    var pageText = page.Text;
                    
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        text.AppendLine($"--- Page {i} ---");
                        text.AppendLine(pageText.Trim());
                        text.AppendLine();
                    }
                }

                return text.ToString().Trim();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error extracting text from PDF");
                throw new InvalidOperationException($"Failed to extract text from PDF: {ex.Message}", ex);
            }
        });
    }

    private async Task<string> ExtractTextFromWordDocx(byte[] data, ILogger logger)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var stream = new MemoryStream(data);
                using var doc = WordprocessingDocument.Open(stream, false);
                
                var body = doc.MainDocumentPart?.Document?.Body;
                if (body == null) return string.Empty;

                var text = new StringBuilder();
                
                foreach (var element in body.Elements())
                {
                    if (element is Paragraph paragraph)
                    {
                        var paragraphText = paragraph.InnerText;
                        if (!string.IsNullOrWhiteSpace(paragraphText))
                        {
                            text.AppendLine(paragraphText.Trim());
                        }
                    }
                    else if (element is DocumentFormat.OpenXml.Wordprocessing.Table table)
                    {
                        text.AppendLine("--- Table Content ---");
                        text.AppendLine(ExtractTextFromWordTable(table));
                        text.AppendLine();
                    }
                }

                return text.ToString().Trim();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error extracting text from Word document");
                throw new InvalidOperationException($"Failed to extract text from Word document: {ex.Message}", ex);
            }
        });
    }

    private string ExtractTextFromWordTable(DocumentFormat.OpenXml.Wordprocessing.Table table)
    {
        var tableText = new StringBuilder();
        
        foreach (var row in table.Elements<TableRow>())
        {
            var rowText = new List<string>();
            foreach (var cell in row.Elements<TableCell>())
            {
                rowText.Add(cell.InnerText.Trim());
            }
            tableText.AppendLine(string.Join(" | ", rowText));
        }
        
        return tableText.ToString();
    }

    private async Task<string> ExtractTextFromWordDoc(byte[] data, ILogger logger)
    {
        logger.LogWarning("Legacy .doc format has limited support. Consider converting to .docx");
        return await Task.FromResult("[Legacy .doc format - limited text extraction support. Please convert to .docx for full text extraction.]");
    }

    private async Task<string> ExtractTextFromExcelXlsx(byte[] data, ILogger logger)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var stream = new MemoryStream(data);
                using var doc = SpreadsheetDocument.Open(stream, false);
                
                var workbookPart = doc.WorkbookPart;
                if (workbookPart == null) return string.Empty;

                var text = new StringBuilder();
                var sheets = workbookPart.Workbook.Sheets?.Elements<Sheet>();
                
                if (sheets == null) return string.Empty;

                foreach (var sheet in sheets)
                {
                    text.AppendLine($"=== WORKSHEET: {sheet.Name} ===");
                    
                    var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
                    var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
                    
                    if (sheetData != null)
                    {
                        foreach (var row in sheetData.Elements<Row>())
                        {
                            var rowText = new List<string>();
                            foreach (var cell in row.Elements<Cell>())
                            {
                                var cellValue = GetCellValue(cell, workbookPart);
                                if (!string.IsNullOrEmpty(cellValue))
                                {
                                    rowText.Add(cellValue);
                                }
                            }
                            
                            if (rowText.Any())
                            {
                                text.AppendLine(string.Join(" | ", rowText));
                            }
                        }
                    }
                    text.AppendLine();
                }

                return text.ToString().Trim();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error extracting text from Excel document");
                throw new InvalidOperationException($"Failed to extract text from Excel document: {ex.Message}", ex);
            }
        });
    }

    private string GetCellValue(Cell cell, WorkbookPart workbookPart)
    {
        if (cell.CellValue == null) return string.Empty;

        var value = cell.CellValue.Text;
        
        if (cell.DataType?.Value == CellValues.SharedString)
        {
            var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;
            if (sharedStrings != null && int.TryParse(value, out var index))
            {
                return sharedStrings.Elements<SharedStringItem>().ElementAtOrDefault(index)?.InnerText ?? string.Empty;
            }
        }

        return value;
    }

    private async Task<string> ExtractTextFromExcelXls(byte[] data, ILogger logger)
    {
        logger.LogWarning("Legacy .xls format has limited support. Consider converting to .xlsx");
        return await Task.FromResult("[Legacy .xls format - limited text extraction support. Please convert to .xlsx for full text extraction.]");
    }

    private async Task<string> ExtractTextFromPowerPointPptx(byte[] data, ILogger logger)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var stream = new MemoryStream(data);
                using var doc = PresentationDocument.Open(stream, false);
                
                var presentationPart = doc.PresentationPart;
                if (presentationPart == null) return string.Empty;

                var text = new StringBuilder();
                var slideIdList = presentationPart.Presentation.SlideIdList;
                
                if (slideIdList == null) return string.Empty;

                int slideNumber = 1;
                foreach (var slideId in slideIdList.Elements<SlideId>())
                {
                    var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!);
                    text.AppendLine($"=== SLIDE {slideNumber} ===");
                    
                    var slideText = ExtractTextFromSlide(slidePart);
                    if (!string.IsNullOrEmpty(slideText))
                    {
                        text.AppendLine(slideText);
                    }
                    else
                    {
                        text.AppendLine("[No text content on this slide]");
                    }
                    
                    text.AppendLine();
                    slideNumber++;
                }

                return text.ToString().Trim();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error extracting text from PowerPoint document");
                throw new InvalidOperationException($"Failed to extract text from PowerPoint document: {ex.Message}", ex);
            }
        });
    }

    private string ExtractTextFromSlide(SlidePart slidePart)
    {
        var text = new StringBuilder();
        
        var textElements = slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>();
        foreach (var textElement in textElements)
        {
            var textValue = textElement.Text?.Trim();
            if (!string.IsNullOrEmpty(textValue))
            {
                text.AppendLine(textValue);
            }
        }

        return text.ToString().Trim();
    }

    private async Task<string> ExtractTextFromPowerPointPpt(byte[] data, ILogger logger)
    {
        logger.LogWarning("Legacy .ppt format has limited support. Consider converting to .pptx");
        return await Task.FromResult("[Legacy .ppt format - limited text extraction support. Please convert to .pptx for full text extraction.]");
    }

    private async Task<string> ExtractTextFromPlainText(byte[] data, ILogger logger)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Try UTF-8 first, fall back to other encodings if needed
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                try
                {
                    return Encoding.ASCII.GetString(data);
                }
                catch
                {
                    return Encoding.Default.GetString(data);
                }
            }
        });
    }

    private async Task<string> ExtractTextFromRichText(byte[] data, ILogger logger)
    {
        return await Task.Run(() =>
        {
            var rtfContent = Encoding.UTF8.GetString(data);
            
            // Basic RTF text extraction (remove RTF control codes)
            var plainText = Regex.Replace(rtfContent, @"\\[a-z]+\d*\s?", " ");
            plainText = Regex.Replace(plainText, @"[{}]", "");
            plainText = Regex.Replace(plainText, @"\s+", " ");
            
            return plainText.Trim();
        });
    }

    #endregion
}