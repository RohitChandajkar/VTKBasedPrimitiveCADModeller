using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Shapes;
using Kitware.VTK;  // Required for VTK components



namespace WindowsFormsApp1
{
    public partial class VTKBasedCADModelllerUI : Form
    {    
        private vtkRenderer renderer;           // renderer object
        private vtkActor selectedActor;        //  selectedActor
        private vtkRenderWindowInteractor interactor1; // vtkRenderWindowInteractor object
        private bool isSketchMode = false;      
        private bool isDrawingCircle = false;  
        private bool isDrawingLine = false;   
        private bool isDrawingPoint = false;   
        private bool isDrawingArc = false;     
        private bool isDrawingEllipse = false;  
        private double startX, startY;         
       

        public VTKBasedCADModelllerUI()
        {
            InitializeComponent();
            this.Load += Form1_Load;    // Bind the Load event to a method         
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupRenderWindowControl(); // Call setup during form load
        }

        //To render window controller 
        private void RenderWindowControl1_Load(object sender, EventArgs e)
        {
            this.renderer = vtkRenderer.New();
            this.renderWindowControl1.RenderWindow.AddRenderer(this.renderer);
            this.renderer.ResetCamera();

            // Set up a picker to handle actor selection with mouse clicks
            var picker = vtkPropPicker.New(); // Use vtkPropPicker to pick props (actors)
            this.renderWindowControl1.RenderWindow.GetInteractor().SetPicker(picker);

            // Set the background color
            this.renderer.SetBackground(0.678, 0.847, 0.902); // Light blue
 
            //used for delete selected elements
            this.renderWindowControl1.RenderWindow.GetInteractor().LeftButtonPressEvt += (s, args) =>
            {
                picker.Pick(this.renderWindowControl1.RenderWindow.GetInteractor().GetEventPosition()[0],
                            this.renderWindowControl1.RenderWindow.GetInteractor().GetEventPosition()[1], 0, this.renderer);


                var pickedActor = picker.GetActor(); // Get the picked actor
                if (pickedActor != null)
                {
                    // Deselect the previous actor (resetting color)
                    if (selectedActor != null)
                    {
                        selectedActor.GetProperty().SetColor(0.0, 0.0, 0.0); // Reset to white
                    }

                    // Set the new selected actor and change its color to indicate selection
                    selectedActor = pickedActor;
                    selectedActor.GetProperty().SetColor(1.0, 0.0, 0.0); // Highlight in red
                }

                ConfigureInteraction(); // Call to configure interaction
                this.renderWindowControl1.RenderWindow.Render(); // Re-render to reflect color change
            };
        }


        // To allow user to drag and down primitive shapes 
        private void SetupRenderWindowControl()
        {      
             // Draw the initial plane for XY
             DrawPlane("XY");
             interactor1 = renderWindowControl1.RenderWindow.GetInteractor();

            // Set the background color
            this.renderer.SetBackground(0.678, 0.847, 0.902); // Light blue
            var interactor = renderWindowControl1.RenderWindow.GetInteractor();

            // Capture the start coordinates when left mouse button is pressed
            interactor.LeftButtonPressEvt += (s, args) =>
            {
                var eventPos = interactor.GetEventPosition();
                startX = eventPos[0];
                startY = eventPos[1];
            };
           
            // Handle drawing when the left mouse button is released
            interactor.LeftButtonReleaseEvt += (s, args) =>
            {
                if (isSketchMode)
                {
                    var eventPos = interactor.GetEventPosition();
                    double endX = eventPos[0];
                    double endY = eventPos[1];

                    // Convert screen coordinates to world coordinates
                    var worldStart = new vtkCoordinate();
                    worldStart.SetCoordinateSystemToDisplay();
                    worldStart.SetValue(startX, startY, 0);

                    var worldEnd = new vtkCoordinate();
                    worldEnd.SetCoordinateSystemToDisplay();
                    worldEnd.SetValue(endX, endY, 0);

                    var worldStartPos = worldStart.GetComputedWorldValue(renderer);
                    var worldEndPos = worldEnd.GetComputedWorldValue(renderer);

                    // Calculate the center and radius for the circle
                    double radius = Math.Sqrt(Math.Pow(worldEndPos[0] - worldStartPos[0], 2) + Math.Pow(worldEndPos[1] - worldStartPos[1], 2));
                    double centerX = (worldStartPos[0] + worldEndPos[0]) / 2;
                    double centerY = (worldStartPos[1] + worldEndPos[1]) / 2;

                    // Draw the appropriate shape based on the active flag
                    if (isDrawingCircle)
                    {                      
                        DrawCircle(centerX, centerY, radius);  // Use the computed values
                        isDrawingCircle = false;               // Reset the flag
                    }
                    else if (isDrawingLine)
                    {                        
                        DrawLine(worldStartPos, worldEndPos);  // Use the computed world coordinates
                        isDrawingLine = false;                 // Reset the flag
                    }
                    else if (isDrawingPoint)
                    {                       
                        DrawPoint(worldStartPos);  // Use the start position
                        isDrawingPoint = false;    // Reset the flag
                    }
                    else if (isDrawingArc)
                    {                                           
                        DrawArc(worldStartPos, worldEndPos);  // Use the computed world coordinates
                        isDrawingArc = false;                 // Reset the flag
                    }
                    else if (isDrawingEllipse)
                    {                     
                        DrawEllipse(worldStartPos, worldEndPos);  // Use the computed world coordinates
                        isDrawingEllipse = false;                // Reset the flag
                    }

                    renderWindowControl1.RenderWindow.Render();  // Re-render the scene to apply changes
                }

            };
          
            // Initial render to set up the scene
            renderWindowControl1.RenderWindow.Render();
        }

        //To disable zoom in - zoom out 
        private void ConfigureInteraction()
        {
            try
            {              
                if (isSketchMode)
                {
                    // Use vtkInteractorStyleTrackballActor for sketch mode
                    interactor1.SetInteractorStyle(vtkInteractorStyleTrackballActor.New());
                }
                else
                {
                    // Use vtkInteractorStyleTrackballCamera for view mode
                    vtkInteractorStyleTrackballCamera style = vtkInteractorStyleTrackballCamera.New();
                    style.SetMotionFactor(10); // Adjust motion factor as needed
                    interactor1.SetInteractorStyle(style);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ConfigureInteraction: {ex.Message}");
            }
        }

        //Sketch Button clcik event
        private void SketchButton_Click(object sender, EventArgs e)
        {              
            isSketchMode = true; // Activate sketch mode
            ConfigureInteraction();
            EnableSketchButtons(true);         
        }

         //Viw Button clcik event
        private void ViewButton_Click(object sender, EventArgs e)
        {                    
            isSketchMode = false; // Deactivate sketch mode
            ConfigureInteraction();
            EnableSketchButtons(false);
        }

        // Enable or disable drawing buttons based on the sketch mode
        private void EnableSketchButtons(bool enable)
        {
            this.circleButton.Enabled = enable;
            this.lineButton.Enabled = enable;
            this.pointButton.Enabled = enable;
            this.arcButton.Enabled = enable;
            this.ellipseButton.Enabled = enable;
            
        }

        //Circle Button clcik event
        private void CircleButton_Click(object sender, EventArgs e)
        {
            if (isSketchMode)
            {
                isDrawingCircle = true;  // Set flag to enable drawing circles
            }
        }

        //Line Button clcik event
        private void LineButton_Click(object sender, EventArgs e)
        {
            if (isSketchMode)
            {
                isDrawingLine = true;  // Set flag to enable line drawing
            }
        }

        //Point Button clcik event
        private void PointButton_Click(object sender, EventArgs e)
        {
            if (isSketchMode)
            {
                isDrawingPoint = true;  // Set flag to enable point drawing               
            }
        }

        //Arc Button clcik event
        private void ArcButton_Click(object sender, EventArgs e)
        {
            if (isSketchMode)
            {
                isDrawingArc = true;  // Set flag to enable arc drawing
            }
        }

        //Ellipse Button clcik event
        private void EllipseButton_Click(object sender, EventArgs e)
        {
            if (isSketchMode)
            {
                isDrawingEllipse = true;  // Set flag to enable ellipse drawing
            }
        }

        //Save Button clcik event to save VTK file
        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // writer
                vtkRenderWindow renderWindow = renderWindowControl1.RenderWindow;
                vtkRenderWindowInteractor interactor = renderWindow.GetInteractor();
                vtkWindowToImageFilter windowToImageFilter = vtkWindowToImageFilter.New();
                windowToImageFilter.SetInput(renderWindow);
                windowToImageFilter.Update();

                // Open file dialog to choose save location and filename
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "VTK Files (*.vti)|*.vti|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;

                    // Save the image as a VTK file
                    vtkDataSetWriter writer = vtkDataSetWriter.New();
                    writer.SetFileName(fileName);
                    writer.SetInputConnection(windowToImageFilter.GetOutputPort());
                    writer.Write();

                    // Display success message
                    MessageBox.Show("File saved successfully: " + fileName, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // Display error message
                MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // connector to create circle 
        private void DrawCircle(double centerX, double centerY, double radius)
        {
            var Shape = new Shapes();
            var polyData = Shape.CreateCircle(centerX, centerY, radius);

            // Now that we have the polyData, let's set up the mapper, actor, and renderer
            var mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(polyData.GetProducerPort());

            var actor = vtkActor.New();
            actor.SetMapper(mapper);
            actor.GetProperty().SetColor(0, 0, 0); // Black color

            renderer.AddActor(actor);
            renderer.ResetCamera(); // Ensure the circle is visible
            renderWindowControl1.RenderWindow.Render(); // Render the scene
        }


        // connector to create Line
        private void DrawLine(double[] worldStartPos, double[] worldEndPos)
        {

            if (isSketchMode)
            {
                var lineShape = new Shapes(); // Instantiate the model
                var polyData = lineShape.CreateLine(worldStartPos, worldEndPos); // Get the line's polyData

                // Create the mapper, actor, and add to the renderer
                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetColor(0, 0, 0); // Black color

                renderer.AddActor(actor); // Add the line to the renderer
                renderer.ResetCamera(); // Ensure the line is visible

                renderWindowControl1.RenderWindow.Render(); // Render the updated scene
            }
        }

        // connector to create Point
        private void DrawPoint(double[] worldStartPos)
        {
            if (isSketchMode) // Ensure sketch mode is active
            {
                // Create the point using the provided function
                var pointShape = new Shapes();
                var polyData = pointShape.CreatePoint(worldStartPos);

                // Create mapper for the point
                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                // Create actor for the point
                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetPointSize(100); // Set the point size
                actor.GetProperty().SetColor(0, 0, 0); // Set the point color to black

                // Add the actor to the renderer
                renderer.AddActor(actor);
                renderer.ResetCamera(); // Ensure visibility

                // Set the renderer to the render window if not already set
                if (renderWindowControl1.RenderWindow.HasRenderer(renderer) == 0)
                {
                    renderWindowControl1.RenderWindow.AddRenderer(renderer);
                }

                // Re-render the scene
                renderWindowControl1.RenderWindow.Render();
            }

        }

        // connector to create arc
        private void DrawArc(double[] worldStartPos, double[] worldEndPos)
        {
            if (isSketchMode)  // Check if sketch mode is active
            {
                var arcShape = new Shapes(); // Instantiate the model
                var polyData = arcShape.CreateArc(worldStartPos, worldEndPos); // Create the arc with specified parameters

                // Create a vtkPolyDataMapper to map the polyData
                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                // Create a vtkActor to represent the arc in the scene
                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetColor(0, 0, 0); // Set arc color to black

                // Add the actor to the renderer
                renderer.AddActor(actor);
                renderer.ResetCamera(); // Ensure the arc is in view
                renderWindowControl1.RenderWindow.Render(); // Render the updated scene

            }
        }


        // connector to create Ellipse
        private void DrawEllipse(double[] worldStartPos, double[] worldEndPos)
        {
            if (isSketchMode)  // Check if sketch mode is active
            {
                var ellipseShape = new Shapes(); // Instantiate the model
                var polyData = ellipseShape.CreateEllipse(worldStartPos, worldEndPos); // Get the polyData for the ellipse

                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetColor(0, 0, 0); // Set the ellipse's color to black

                renderer.AddActor(actor); // Add the ellipse to the renderer
             //   renderer.ResetCamera(); // Ensure the ellipse is visible
                renderWindowControl1.RenderWindow.Render(); // Re-render the s
            }
        }

        // Delete Button click event 
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (selectedActor != null) // If an actor is selected
            {
                this.renderer.RemoveActor(selectedActor); // Remove the actor
                selectedActor = null; // Reset the selected actor
                this.renderer.ResetCamera(); // Reset the camera
                this.renderWindowControl1.RenderWindow.Render(); // Re-render the scene
            }
        }

        // To removes all actors from the renderer.
        private void RemoveAllActors()
        {
            // Get all the actors in the renderer
            var actorsCollection = renderer.GetActors();

            // Iterate through all actors and remove them
            var actors = new List<vtkActor>(); // Use a list to store actors safely

            // Retrieve each actor from the collection and cast to vtkActor or vtkProp
            for (int i = 0; i < actorsCollection.GetNumberOfItems(); i++)
            {
                // Ensure correct type by casting
                var actor = actorsCollection.GetItemAsObject(i) as vtkProp;

                if (actor != null)  // Confirm casting was successful
                {
                    actors.Add((vtkActor)actor);  // Add actor to the list
                }
            }

            // Now safely remove all actors from the renderer
            foreach (var actor in actors)
            {
                renderer.RemoveActor(actor);
            }
        }

        // To draw plain 
        private void DrawPlane(string plane)
        {
            // Create a simple grid to represent the plane
            var planeSource = vtkPlaneSource.New();
            planeSource.SetXResolution(10);  // Set resolution for the grid
            planeSource.SetYResolution(10);


            var mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(planeSource.GetOutputPort());

            var actor = vtkActor.New();
            actor.SetMapper(mapper);
            actor.GetProperty().SetRepresentationToWireframe();  // Show as wireframe

            // Adjust the orientation and position based on the selected plane
            switch (plane)
            {
                case "XY":
                    actor.SetOrientation(0, 0, 0);  // No rotation for XY
                    actor.SetPosition(0, 0, 0);     // Position at origin
                    break;

                case "YZ":
                    actor.SetOrientation(0, 90, 0);  // Rotate for YZ
                    actor.SetPosition(0, 0, 0);     // Position at origin
                    break;

                case "XZ":
                    actor.SetOrientation(90, 0, 0);  // Rotate for XZ
                    actor.SetPosition(0, 0, 0);     // Position at origin
                    break;
            }

            actor.PickableOff(); // Prevent selection or interaction with the plane


            renderer.AddActor(actor);  // Add the plane to the renderer
        }

       
        // XY button click event 
        private void XYButton_Click(object sender, EventArgs e)
        {
            // Clear all actors before setting the new plane
            RemoveAllActors();  // Custom method to remove all actors
            DrawPlane("XY");

            var camera = renderer.GetActiveCamera();
            camera.SetPosition(0, 0, 1);  // Look down from the Z-axis
            camera.SetViewUp(0, 1, 0);   // Orient along the Y-axis

            // Set parallel projection scale to control zoom level
            camera.SetParallelScale(300);  // Adjust to achieve desired zoom

            var interactor = renderWindowControl1.RenderWindow.GetInteractor();
            interactor.SetInteractorStyle(vtkInteractorStyleTrackballCamera.New()); // Use fixed camera
            renderWindowControl1.RenderWindow.Render();  // Re-render the scene
        }


        //YZ button click event 
        private void YZButton_Click(object sender, EventArgs e)
        {
            RemoveAllActors();  // Custom method to remove all actors
            DrawPlane("YZ");

            var camera = renderer.GetActiveCamera();
            camera.SetPosition(1, 0, 0);  // Look down from the X-axis
            camera.SetViewUp(0, 0, 1);   // Orient along Z-axis

            // Set parallel projection scale to control zoom level
            camera.SetParallelScale(300);  // Adjust to achieve desired zoom

            var interactor = renderWindowControl1.RenderWindow.GetInteractor();
            interactor.SetInteractorStyle(vtkInteractorStyleTrackballCamera.New()); // Use fixed camera
            renderWindowControl1.RenderWindow.Render();  // Re-render the scene
        }

        //XZ Button click event
        private void ZXButton_Click(object sender, EventArgs e)
        {
            RemoveAllActors();  // Custom method to remove all actors
            DrawPlane("XZ");

            var camera = renderer.GetActiveCamera();
            camera.SetPosition(0, 1, 0);  // Look down from the Y-axis
            camera.SetViewUp(0, 0, 1);   // Orient along Z-axis

            // Set parallel projection scale to control zoom level
            camera.SetParallelScale(300);  // Adjust to achieve desired zoom

            var interactor = renderWindowControl1.RenderWindow.GetInteractor();
            interactor.SetInteractorStyle(vtkInteractorStyleTrackballCamera.New()); // Use fixed camera

            renderWindowControl1.RenderWindow.Render();  // Re-render the scene
        }
        

    }
}
