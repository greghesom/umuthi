// Workflow Designer JavaScript Interop
window.workflowDesigner = {
    dotNetObjectRef: null,

    initialize: function (dotNetObjectRef) {
        this.dotNetObjectRef = dotNetObjectRef;
        
        // Add global keyboard event listener
        document.addEventListener('keydown', this.handleKeyDown.bind(this));
        
        console.log('Workflow Designer initialized');
    },

    handleKeyDown: function (e) {
        if (this.dotNetObjectRef) {
            // Only handle delete key for now
            if (e.key === 'Delete' || e.key === 'Backspace') {
                // Prevent default browser behavior
                e.preventDefault();
                
                // Only trigger if focus is not on an input/textarea
                if (!['INPUT', 'TEXTAREA'].includes(e.target.tagName)) {
                    this.dotNetObjectRef.invokeMethod('OnKeyDown', e.key);
                }
            }
        }
    },

    dispose: function () {
        // Remove event listeners
        document.removeEventListener('keydown', this.handleKeyDown.bind(this));
        this.dotNetObjectRef = null;
        
        console.log('Workflow Designer disposed');
    }
};