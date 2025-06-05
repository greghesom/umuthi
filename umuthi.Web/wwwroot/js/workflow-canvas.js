// Workflow Canvas JavaScript Interop
window.workflowCanvas = {
    canvasRef: null,
    dotNetRef: null,
    isMouseDown: false,
    touchStart: null,
    rafId: null,

    initialize: function (canvasElement, dotNetObjectRef) {
        this.canvasRef = canvasElement;
        this.dotNetRef = dotNetObjectRef;
        this.setupEventListeners();
    },

    setupEventListeners: function () {
        if (!this.canvasRef) return;

        // Mouse events
        this.canvasRef.addEventListener('mousedown', this.handleMouseDown.bind(this));
        this.canvasRef.addEventListener('mousemove', this.handleMouseMove.bind(this));
        this.canvasRef.addEventListener('mouseup', this.handleMouseUp.bind(this));
        this.canvasRef.addEventListener('mouseleave', this.handleMouseUp.bind(this));
        this.canvasRef.addEventListener('wheel', this.handleWheel.bind(this), { passive: false });

        // Touch events for mobile
        this.canvasRef.addEventListener('touchstart', this.handleTouchStart.bind(this), { passive: false });
        this.canvasRef.addEventListener('touchmove', this.handleTouchMove.bind(this), { passive: false });
        this.canvasRef.addEventListener('touchend', this.handleTouchEnd.bind(this));

        // Prevent context menu
        this.canvasRef.addEventListener('contextmenu', function (e) {
            e.preventDefault();
        });

        // Prevent text selection during drag
        this.canvasRef.addEventListener('selectstart', function (e) {
            e.preventDefault();
        });
    },

    handleMouseDown: function (e) {
        // Only handle left mouse button
        if (e.button !== 0) return;
        
        e.preventDefault();
        this.isMouseDown = true;
        
        const rect = this.canvasRef.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        
        this.dotNetRef.invokeMethodAsync('OnMouseDown', x, y);
    },

    handleMouseMove: function (e) {
        if (!this.isMouseDown) return;
        
        e.preventDefault();
        
        // Use requestAnimationFrame for smooth updates
        if (this.rafId) {
            cancelAnimationFrame(this.rafId);
        }
        
        this.rafId = requestAnimationFrame(() => {
            const rect = this.canvasRef.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
            this.dotNetRef.invokeMethodAsync('OnMouseMove', x, y);
        });
    },

    handleMouseUp: function (e) {
        if (!this.isMouseDown) return;
        
        this.isMouseDown = false;
        
        if (this.rafId) {
            cancelAnimationFrame(this.rafId);
            this.rafId = null;
        }
        
        this.dotNetRef.invokeMethodAsync('OnMouseUp');
    },

    handleWheel: function (e) {
        e.preventDefault();
        
        const rect = this.canvasRef.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;
        
        // Normalize wheel delta for different browsers
        let delta = 0;
        if (e.deltaY) {
            delta = e.deltaY;
        } else if (e.detail) {
            delta = e.detail * 40;
        } else if (e.wheelDelta) {
            delta = -e.wheelDelta;
        }
        
        this.dotNetRef.invokeMethodAsync('OnWheel', delta, x, y);
    },

    handleTouchStart: function (e) {
        e.preventDefault();
        
        if (e.touches.length === 1) {
            // Single touch - pan
            const touch = e.touches[0];
            const rect = this.canvasRef.getBoundingClientRect();
            const x = touch.clientX - rect.left;
            const y = touch.clientY - rect.top;
            
            this.touchStart = { x, y };
            this.dotNetRef.invokeMethodAsync('OnMouseDown', x, y);
        } else if (e.touches.length === 2) {
            // Two fingers - prepare for zoom
            this.touchStart = {
                touch1: { x: e.touches[0].clientX, y: e.touches[0].clientY },
                touch2: { x: e.touches[1].clientX, y: e.touches[1].clientY },
                distance: this.getTouchDistance(e.touches[0], e.touches[1])
            };
        }
    },

    handleTouchMove: function (e) {
        e.preventDefault();
        
        if (e.touches.length === 1 && this.touchStart && !this.touchStart.touch1) {
            // Single touch - pan
            const touch = e.touches[0];
            const rect = this.canvasRef.getBoundingClientRect();
            const x = touch.clientX - rect.left;
            const y = touch.clientY - rect.top;
            
            if (this.rafId) {
                cancelAnimationFrame(this.rafId);
            }
            
            this.rafId = requestAnimationFrame(() => {
                this.dotNetRef.invokeMethodAsync('OnMouseMove', x, y);
            });
        } else if (e.touches.length === 2 && this.touchStart && this.touchStart.touch1) {
            // Two fingers - zoom
            const currentDistance = this.getTouchDistance(e.touches[0], e.touches[1]);
            const scale = currentDistance / this.touchStart.distance;
            
            // Calculate center point
            const centerX = (e.touches[0].clientX + e.touches[1].clientX) / 2;
            const centerY = (e.touches[0].clientY + e.touches[1].clientY) / 2;
            const rect = this.canvasRef.getBoundingClientRect();
            const x = centerX - rect.left;
            const y = centerY - rect.top;
            
            // Simulate wheel event for zoom
            const delta = scale > 1 ? -100 : 100;
            this.dotNetRef.invokeMethodAsync('OnWheel', delta, x, y);
            
            this.touchStart.distance = currentDistance;
        }
    },

    handleTouchEnd: function (e) {
        if (e.touches.length === 0) {
            // All touches ended
            this.touchStart = null;
            
            if (this.rafId) {
                cancelAnimationFrame(this.rafId);
                this.rafId = null;
            }
            
            this.dotNetRef.invokeMethodAsync('OnMouseUp');
        } else if (e.touches.length === 1 && this.touchStart && this.touchStart.touch1) {
            // Switched from two finger to one finger
            const touch = e.touches[0];
            const rect = this.canvasRef.getBoundingClientRect();
            const x = touch.clientX - rect.left;
            const y = touch.clientY - rect.top;
            
            this.touchStart = { x, y };
            this.dotNetRef.invokeMethodAsync('OnMouseDown', x, y);
        }
    },

    getTouchDistance: function (touch1, touch2) {
        const dx = touch1.clientX - touch2.clientX;
        const dy = touch1.clientY - touch2.clientY;
        return Math.sqrt(dx * dx + dy * dy);
    },

    dispose: function () {
        if (this.rafId) {
            cancelAnimationFrame(this.rafId);
        }
        this.canvasRef = null;
        this.dotNetRef = null;
        this.isMouseDown = false;
        this.touchStart = null;
    }
};