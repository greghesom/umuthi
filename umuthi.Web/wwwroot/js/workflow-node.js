// Workflow Node JavaScript Interop
window.workflowNode = {
    currentDragNode: null,
    isDragging: false,

    startDrag: function (dotNetObjectRef) {
        this.currentDragNode = dotNetObjectRef;
        this.isDragging = true;
        
        // Add global event listeners
        document.addEventListener('mousemove', this.handleMouseMove.bind(this));
        document.addEventListener('mouseup', this.handleMouseUp.bind(this));
        
        // Prevent text selection during drag
        document.body.style.userSelect = 'none';
        
        // Add grabbing cursor
        document.body.style.cursor = 'grabbing';
    },

    handleMouseMove: function (e) {
        if (this.isDragging && this.currentDragNode) {
            e.preventDefault();
            this.currentDragNode.invokeMethod('OnMouseMove', e.clientX, e.clientY);
        }
    },

    handleMouseUp: function (e) {
        if (this.isDragging && this.currentDragNode) {
            this.currentDragNode.invokeMethod('OnMouseUp');
            this.stopDrag();
        }
    },

    stopDrag: function () {
        this.isDragging = false;
        this.currentDragNode = null;
        
        // Remove global event listeners
        document.removeEventListener('mousemove', this.handleMouseMove.bind(this));
        document.removeEventListener('mouseup', this.handleMouseUp.bind(this));
        
        // Restore text selection
        document.body.style.userSelect = '';
        
        // Restore cursor
        document.body.style.cursor = '';
    }
};