using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpResizeAdorner : Adorner {
        /// <summary>
        /// Element that can resize.  
        /// </summary>
        public FrameworkElement adornedElement;

        /// <summary>
        /// Auto-generated element.  Wraps adornedElement and provides reference position during resize.   
        /// </summary>
        public Canvas parentCanvas = new Canvas();

        /// <summary>
        /// Application window.  Class finds this automatically.  
        /// </summary>
        public Window window_;

        /// <summary>
        /// Max height allowed for adornedElement.  In pixels.
        /// </summary>
        public double _maxHeight;

        /// <summary>
        /// Max width allowed for adornedElement.  In pixels.
        /// </summary>
        public double _maxWidth;

        public double _minWidth, _minHeight;

        public double _defWidth, _defHeight;
        // Resizing adorner uses Thumbs for visual elements.  Thumbs have built-in mouse input handling.
        public Thumb bottomRight = new Thumb();

        /// <summary>
        /// To store and manage the adorner's visual children.
        /// </summary>
        VisualCollection visualChildren;




        /// <summary>
        /// Enables resizing on element.  
        /// Adds resize handle to bottom right corner.    
        /// </summary>
        /// <example>
        /// Very simple one-line call.   
        /// new CResizeAdorner(myButton, 200, 200); //Allow resizing myButton to max 200px.    
        /// </example>
        /// <remarks>
        /// Modified from: https://denisvuyka.wordpress.com/2007/10/15/wpf-simple-adorner-usage-with-drag-and-resize-operations/#comments
        /// Tested: 2017-06-03 all passed.
        /// </remarks>
        /// <param name="_adornedElement">Element to apply resize ability to.  </param>
        /// <param name="maxHeight">Max height allowed when resizing.  In pixels. </param>
        /// <param name="maxWidth">Max width allowed when resizing.  In pixels.  </param>
        /// Tested: 2017-06-03 all passed.  
        public MpResizeAdorner(FrameworkElement _adornedElement, double minWidth, double minHeight,  double maxWidth, double maxHeight) : base(_adornedElement) //Needs this "base" snippet when extending Adorner class.   
        {
            if (_adornedElement.IsLoaded == false)
            {
                MessageBox.Show("Error CResizeAdorner: Element " + _adornedElement.Name + " not loaded.  Exiting. ");
                return;
            }

            visualChildren = new VisualCollection(this);

            //From constructor.  
            adornedElement = _adornedElement;

            _defWidth = adornedElement.Width;
            _defHeight = adornedElement.Height;

            _maxHeight = maxHeight;
            _maxWidth = maxWidth;
            
            _minHeight = minHeight;
            _minWidth = minWidth;

            window_ = Window.GetWindow(adornedElement);

            //Set parent canvas size equal to resize element.      
            parentCanvas.Height = adornedElement.RenderSize.Height;
            parentCanvas.Width = adornedElement.RenderSize.Width;

            //Debug only, to view canvas.
            //parentCanvas.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#faaaaa"));
            //parentCanvas.Background.Opacity = 0.5; //Making invisible is OK.  

            //Move resize element inside parent canvas.  
            DependencyObject parentOld = VisualTreeHelper.GetParent(adornedElement);
            parentOld.DisconnectChild(adornedElement); //Element cannot have two parents. 
            parentOld.AddChild(parentCanvas);
            parentCanvas.AddChild(adornedElement);

            //Transfer alignment from adornedElement into parentCanvas.  
            parentCanvas.HorizontalAlignment = adornedElement.HorizontalAlignment;
            parentCanvas.VerticalAlignment = adornedElement.VerticalAlignment;

            //Transfer margin settings (left,right,top,down) from adornedElement into parentCanvas.  
            parentCanvas.Margin = new Thickness(
                adornedElement.Margin.Left,
                adornedElement.Margin.Top,
                adornedElement.Margin.Bottom,
                adornedElement.Margin.Right
                );
            //Remove margins from child, otherwise it will offset from parent canvas.  
            adornedElement.Margin = new Thickness(0, 0, 0, 0);

            //Set adornedElement position to upper left.  This is required for resizing (gives it an anchor to reference).  Prevents both sides from shrinking when resizing element with mouse.  
            Canvas.SetTop(adornedElement, 0.0);
            Canvas.SetLeft(adornedElement, 0.0);

            //Apply layer to element.  
            AdornerLayer aLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            aLayer.Add(this);

            //Create corner icon.
            BuildAdornerCorner();

            Enable();
        }

        public void Enable() {
            //Handles resizing event.  
            bottomRight.DragDelta += new DragDeltaEventHandler(HandleResizing);

            //Stop resizing event.  
            bottomRight.DragCompleted += delegate {
                //Shrink canvas back to element's size.      
                parentCanvas.Height = adornedElement.RenderSize.Height;
                parentCanvas.Width = adornedElement.RenderSize.Width;
            };
        }

        public void Disable() {
            //Handles resizing event.  
            bottomRight.DragDelta -= new DragDeltaEventHandler(HandleResizing);

            //Stop resizing event.  
            bottomRight.DragCompleted -= delegate {
                //Shrink canvas back to element's size.      
                parentCanvas.Height = adornedElement.RenderSize.Height;
                parentCanvas.Width = adornedElement.RenderSize.Width;
            };
        }



        /// <summary>
        /// Handler for resizing from the bottom-right.
        /// </summary>
        /// <param name="sender">From event.</param>
        /// <param name="args">From event.</param>
        /// Tested: 2017-06-03 all passed.  
        public void HandleResizing(object sender, DragDeltaEventArgs args)
        {
            Thumb hitThumb = sender as Thumb;

            //First, set minimum values.    
            //RenderSize is best.  Native Height/Width occassionaly returns weird NaN values (don't want).  DesiredSize includes margin sizes (don't want).  
            var heightResize = Math.Max(adornedElement.RenderSize.Height + args.VerticalChange, hitThumb.RenderSize.Height);
            var widthResize = Math.Max(adornedElement.RenderSize.Width + args.HorizontalChange, hitThumb.RenderSize.Width);

            //Second, set max values.  
            //Also resizes element.  
            adornedElement.Height = Math.Max(_minHeight, Math.Min(heightResize, _maxHeight));
            adornedElement.Width = Math.Max(_minWidth, Math.Min(widthResize, _maxWidth));

            //Resize parent canvas so it has room to grow.  
            parentCanvas.Height = adornedElement.RenderSize.Height + 100; //Need at least 60px for fast mouse moves.  100px handles very fast moves.  
            parentCanvas.Width = adornedElement.RenderSize.Width + 100;
        }


        /// <summary>
        /// Moves drag icon to correct position.  
        /// </summary>
        /// Tested: 2017-06-03 all passed.  
        public void PositionThumb()
        {
            //RenderSize is best.  Native Height/Width return weird NaN values.  DesiredSize includes margin sizes (don't want).      
            var elemHeight = adornedElement.RenderSize.Height;
            var elemWidth = adornedElement.RenderSize.Width;

            //Fiddled with this to get it lined up perfectly. Places thumb in element's lower right corner.
            bottomRight.Arrange(new Rect(
                (elemWidth - bottomRight.Height) / 2, //Placement
                (elemHeight - bottomRight.Width) / 2, //Placement
                elemWidth,      //Size
                elemHeight    //Size
                ));
        }




        /// <summary>
        /// Mainly to hook into "arrange elements on screen" event.  
        /// </summary>
        /// <param name="finalSize">Not used, only defined for override method.  </param>
        /// Tested: 2017-06-03 all passed.  
        protected override Size ArrangeOverride(Size finalSize)
        {
            PositionThumb();

            //Return the final size.  Not used but need for this override.      
            return finalSize;
        }


        /// <summary>
        /// Creates drag icon and set cursor property.   
        /// </summary>
        /// Tested: 2017-06-03 all passed.  
        public void BuildAdornerCorner()
        {
            //Grab xaml file that draws a little triangle.  
            ResourceDictionary myDictionary = Application.LoadComponent(new Uri("/MpWpfApp;component/Controls/Adorners/Resizer/MpResizeAdornerTriangle.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary;
            bottomRight.Style = (Style)myDictionary["ResizingAdorner_ThumbTriangle"];

            //Visual characteristics.
            bottomRight.Cursor = Cursors.SizeNWSE;
            bottomRight.Height = 14;    //If updating size, also update CResizeAdornerTriangle.xaml.  
            bottomRight.Width = 14;     //If updating size, also update CResizeAdornerTriangle.xaml. 
            bottomRight.Opacity = .5;

            visualChildren.Add(bottomRight);
        }

        //From original author.  These are needed. -RR
        //Override the VisualChildrenCount and GetVisualChild properties to interface with 
        //the adorner's visual collection.
        protected override int VisualChildrenCount { get { return visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }
    }
}
