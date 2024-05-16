using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;
using Kitware.VTK;
using WindowsFormsApp1.Modal;  // Required for VTK components



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
        private Dictionary<vtkActor, object> actorShapeMapping = new Dictionary<vtkActor, object>();   // private vtkActor selectedActor1;  
        public delegate void ShapeClickedEventHandler(object sender, EventArgs e);  // Define delegate for shape clicked event    
        public event ShapeClickedEventHandler ShapeClicked;  // Define event for shape clicked
        private bool isHandlingShapeClick = false; //for stop infinite loop
        private bool isUpdatingPropertiesPanel = false; //for stop infinite loop                                                       
        private Button closeButton; // Add this at the beginning of your class
        private bool isDragging = false;
        private Point dragStartPoint; // This stores the initial position when dragging starts
      
       
        public VTKBasedCADModelllerUI()
        {
            InitializeComponent();          
            this.Load += Form1_Load;    // Bind the Load event to a method
            InitializePropertiesPanel();   //for property panel

            // Subscribe to the ShapeClicked event
            this.ShapeClicked += HandleShapeClick;

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
                        selectedActor.GetProperty().SetColor(0.0, 0.0, 0.0); // Reset to black
                    }

                    // Set the new selected actor and change its color to indicate selection
                    selectedActor = pickedActor;
                    selectedActor.GetProperty().SetColor(1.0, 0.0, 0.0); // Highlight in red

                    // Raise the shape clicked event
                    OnShapeClicked(pickedActor);  // delegant for property panel 
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

        //newcode to show peroperties 
        private void InitializePropertiesPanel()
        {
          
            // Initialize propertiesPanel
            propertiesPanel = new Panel();
            propertiesPanel.Location = new System.Drawing.Point(this.ClientSize.Width - 350, 80);
            propertiesPanel.Size = new System.Drawing.Size(250, this.ClientSize.Height - 214);
            propertiesPanel.BorderStyle = BorderStyle.FixedSingle;
            propertiesPanel.BackColor = System.Drawing.Color.LightGray;

            // Add close button to propertiesPanel
            closeButton = new Button();
            closeButton.Text = "X";
            closeButton.Size = new System.Drawing.Size(20, 20);
            closeButton.Location = new System.Drawing.Point(propertiesPanel.Width - 25, 5);
            closeButton.Click += (sender, e) =>
            {
                propertiesPanel.Visible = false;
            };
            propertiesPanel.Controls.Add(closeButton);

            // Add propertiesPanel to the form's controls
            this.Controls.Add(propertiesPanel);

            // Handle the Resize event of the form
            this.Resize += (sender, e) =>
            {
                propertiesPanel.Location = new System.Drawing.Point(this.ClientSize.Width - 350, 80);
                propertiesPanel.Size = new System.Drawing.Size(250, this.ClientSize.Height - 214);
                closeButton.Location = new System.Drawing.Point(propertiesPanel.Width - 25, 5);
            };

            propertiesPanel.Visible = false; // Start with the panel hidden

            // Add mouse event handlers for dragging
            propertiesPanel.MouseDown += PropertiesPanel_MouseDown;
            propertiesPanel.MouseMove += PropertiesPanel_MouseMove;
            propertiesPanel.MouseUp += PropertiesPanel_MouseUp;
       
        }



        //Handles the MouseDown event for the propertiesPanel to enable dragging.
        private void PropertiesPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location; // Store the initial mouse position
            }
        }

        //To handle property panel floating movement handle event 
        private void PropertiesPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Calculate the new location of the panel
                Point newLocation = propertiesPanel.Location;
                newLocation.X += e.X - dragStartPoint.X;
                newLocation.Y += e.Y - dragStartPoint.Y;
                propertiesPanel.Location = newLocation; // Update the panel's location
            }
        }

        private void PropertiesPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false; // Stop dragging
            }
        }

        public void HandleShapeClick(object sender, EventArgs e)
        {
            if (isHandlingShapeClick)
            {
                return;
            }

            isHandlingShapeClick = true;

            // Handle the shape click event
            if (sender is vtkActor actor)
            {

            
                // Additional logic to handle the shape click              
                UpdatePropertiesPanel(actor, actorShapeMapping[actor]);
                propertiesPanel.Visible = true; // Show the panel when a shape is clicked
                propertiesPanel.BringToFront(); // Bring the panel to the front
            }
            else
            {
                Console.WriteLine("Clicked object is not a recognized shape.");
            }

            isHandlingShapeClick = false;
        }

        // To update constraint of shapes 
        private void UpdatePropertiesPanel(vtkActor actor, object shape)
        {
          
            if (isUpdatingPropertiesPanel)
                return;

            isUpdatingPropertiesPanel = true;

            try
            {
                // Check if a shape is selected
                if (shape != null)
                {
                    // Clear existing controls from the panel
                    propertiesPanel.Controls.Clear();
                    propertiesPanel.Controls.Add(closeButton); // Re-add the close button

                    // Create dictionaries to store updated values from text boxes
                    var updatedValues = new Dictionary<string, double>();

                    // Add controls based on the type of shape
                    if (shape is CircleShape circle)
                    {
                        updatedValues["CenterX"] = circle.CenterX;
                        updatedValues["CenterY"] = circle.CenterY;
                        updatedValues["Radius"] = circle.Radius;

                        AddLabelAndTextBox("X:", circle.CenterX.ToString(), 10, "CenterX", updatedValues);
                        AddLabelAndTextBox("Y:", circle.CenterY.ToString(), 40, "CenterY", updatedValues);
                        AddLabelAndTextBox("Radius:", circle.Radius.ToString(), 70, "Radius", updatedValues);

                        // Add Apply button
                        AddApplyButton(100, (s, e) =>
                        {
                            circle.CenterX = updatedValues["CenterX"];
                            circle.CenterY = updatedValues["CenterY"];
                            circle.Radius = updatedValues["Radius"];
                            UpdateCircleShape(actor, circle); // Update visualization
                        });
                    }
                    else if (shape is LineShape line)
                    {
                        updatedValues["Point1X"] = line.Point1X;
                        updatedValues["Point1Y"] = line.Point1Y;
                        updatedValues["Point2X"] = line.Point2X;
                        updatedValues["Point2Y"] = line.Point2Y;

                        AddLabelAndTextBox("Point1 X:", line.Point1X.ToString(), 10, "Point1X", updatedValues);
                        AddLabelAndTextBox("Point1 Y:", line.Point1Y.ToString(), 40, "Point1Y", updatedValues);
                        AddLabelAndTextBox("Point2 X:", line.Point2X.ToString(), 70, "Point2X", updatedValues);
                        AddLabelAndTextBox("Point2 Y:", line.Point2Y.ToString(), 100, "Point2Y", updatedValues);

                        // Add Apply button
                        AddApplyButton(130, (s, e) =>
                        {
                            line.Point1X = updatedValues["Point1X"];
                            line.Point1Y = updatedValues["Point1Y"];
                            line.Point2X = updatedValues["Point2X"];
                            line.Point2Y = updatedValues["Point2Y"];
                            UpdateLineShape(actor, line); // Update visualization
                        });
                    }
                    else if (shape is PointShape point)
                    {
                        updatedValues["X"] = point.X;
                        updatedValues["Y"] = point.Y;

                        AddLabelAndTextBox("X:", point.X.ToString(), 10, "X", updatedValues);
                        AddLabelAndTextBox("Y:", point.Y.ToString(), 40, "Y", updatedValues);

                        // Add Apply button
                        AddApplyButton(70, (s, e) =>
                        {
                            point.X = updatedValues["X"];
                            point.Y = updatedValues["Y"];
                            UpdatePointShape(actor, point); // Update visualization
                        });
                    }
                    else if (shape is ArcShape arc)
                    {
                        // Handle ArcShape accordingly
                    }
                    else if (shape is EllipseShape ellipse)
                    {
                        updatedValues["Point1X"] = ellipse.Point1X;
                        updatedValues["Point1Y"] = ellipse.Point1Y;
                        updatedValues["Point2X"] = ellipse.Point2X;
                        updatedValues["Point2Y"] = ellipse.Point2Y;

                        AddLabelAndTextBox("Point1 X:", ellipse.Point1X.ToString(), 10, "Point1X", updatedValues);
                        AddLabelAndTextBox("Point1 Y:", ellipse.Point1Y.ToString(), 40, "Point1Y", updatedValues);
                        AddLabelAndTextBox("Point2 X:", ellipse.Point2X.ToString(), 70, "Point2X", updatedValues);
                        AddLabelAndTextBox("Point2 Y:", ellipse.Point2Y.ToString(), 100, "Point2Y", updatedValues);

                        // Add Apply button
                        AddApplyButton(130, (s, e) =>
                        {
                            ellipse.Point1X = updatedValues["Point1X"];
                            ellipse.Point1Y = updatedValues["Point1Y"];
                            ellipse.Point2X = updatedValues["Point2X"];
                            ellipse.Point2Y = updatedValues["Point2Y"];
                            UpdateEllipseShape(actor, ellipse); // Update visualization
                        });
                    }
                }
            }
            finally
            {
                isUpdatingPropertiesPanel = false;
            }
        }


        private void AddLabelAndTextBox(string labelText, string textBoxText, int y, string key, Dictionary<string, double> updatedValues)
        {
            // Calculate the x position for the label and text box
            int labelX = propertiesPanel.Width - 180;
            int textBoxX = propertiesPanel.Width - 100;

            // Create and configure the label
            Label label = new Label();
            label.Text = labelText;
            label.AutoSize = true;
            label.Location = new System.Drawing.Point(labelX, y);
            propertiesPanel.Controls.Add(label);

            // Create and configure the text box
            TextBox textBox = new TextBox();
            textBox.Text = textBoxText;
            textBox.Size = new System.Drawing.Size(80, 20);
            textBox.Location = new System.Drawing.Point(textBoxX, y);
            textBox.TextChanged += (s, e) =>
            {
                if (double.TryParse((s as TextBox).Text, out double value))
                {
                    updatedValues[key] = value;
                }
            };
            propertiesPanel.Controls.Add(textBox);
        }

        private void AddApplyButton(int y, EventHandler clickHandler)
        {
            Button applyButton = new Button();
            applyButton.Text = "Apply";
            applyButton.Size = new System.Drawing.Size(80, 30);
            applyButton.Location = new System.Drawing.Point(propertiesPanel.Width - 100, y);
            applyButton.Click += clickHandler;
            propertiesPanel.Controls.Add(applyButton);
        }


        private void OnShapeClicked(vtkActor actor)
        {
            if (!isHandlingShapeClick)
            {              
                selectedActor = actor;  // Set the selected actor
                ShapeClicked?.Invoke(actor, EventArgs.Empty); // Raise the shape clicked event
            }
        }

        // Method to add event listeners to the shapes in the renderer
        private void AddShapeClickHandlers(vtkActor actor, object shape)
        {            
            // Check if the render window control has been initialized
            if (renderWindowControl1 != null && renderWindowControl1.RenderWindow != null)
            {
                // Get the interactor associated with the render window
                vtkRenderWindowInteractor interactor = renderWindowControl1.RenderWindow.GetInteractor();

                // Attach a click event handler to the interactor
                interactor.LeftButtonPressEvt += (sender, args) =>
                {
                    OnShapeClicked(actor);
                };
            }

            // Add the actor and shape to the mapping dictionary
            actorShapeMapping[actor] = shape;
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

                    // Variables to keep track of last mouse position
                    int[] lastPos = null;

                    // Set up mouse events for screen rotation
                    interactor1.MouseMoveEvt += (s, args) =>
                    {
                        // Get current mouse position
                        int[] pos = interactor1.GetEventPosition();

                        if (lastPos == null)
                        {
                            // Initialize lastPos on the first event
                            lastPos = pos;
                            return;
                        }

                        // Check if the Shift key is pressed
                        int shiftKeyState = interactor1.GetShiftKey();
                        bool isShiftKeyPressed = (shiftKeyState != 0); // Check if shiftKeyState is not equal to 0

                        // Output the shiftKeyState for debugging
                        Console.WriteLine("Shift key state: " + shiftKeyState);

                        // Proceed with rotation if the Shift key is pressed
                        if (isShiftKeyPressed)
                        {
                            int dx = pos[0] - lastPos[0];
                            int dy = pos[1] - lastPos[1];

                            // Rotate the camera based on mouse movement
                            renderer.GetActiveCamera().Azimuth(dx * 0.5);
                            renderer.GetActiveCamera().Elevation(dy * 0.5);
                            renderer.ResetCameraClippingRange();
                            renderWindowControl1.RenderWindow.Render();
                        }

                        // Update lastPos to current position
                        lastPos = pos;
                    };

                    // Reset lastPos when mouse button is pressed
                    interactor1.LeftButtonPressEvt += (s, args) =>
                    {
                        lastPos = null;
                    };

                    interactor1.RightButtonPressEvt += (s, args) =>
                    {
                        lastPos = null;
                    };

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ConfigureInteraction: {ex.Message}");
            }
        }


        //sketch toostrip
        private void SketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isSketchMode = true; // Activate sketch mode
            ShowPanel(sketchPanel);         
            ConfigureInteraction();
            Console.WriteLine("sketch");
        }

        //View toostrip
        private void ViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isSketchMode = false; // Deactivate sketch mode
            ShowPanel(viewPanel);      
            ConfigureInteraction();
           
        }

        //plain toostrip
        private void PlainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowPanel(plainPanel);
        }

        //Save toostrip
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowPanel(savePanel);
            Console.WriteLine("sketchtoolstrip");
        }

        //Delete toostrip
        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowPanel(deletePanel);
        }

        //show panel
        private void ShowPanel(Panel panelToShow)
        {
            sketchPanel.Visible = panelToShow == sketchPanel;
            plainPanel.Visible = panelToShow == plainPanel;
            viewPanel.Visible = panelToShow == viewPanel;
            savePanel.Visible = panelToShow == savePanel;
            deletePanel.Visible = panelToShow == deletePanel;
        }


        private void sketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isSketchMode = true; // Activate sketch mode
            ShowPanel(sketchPanel);
        }

        private void plainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowPanel(plainPanel);
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isSketchMode = false; // Activate sketch mode
            ShowPanel(viewPanel);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowPanel(savePanel);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowPanel(deletePanel);
        }



        //Viw Button clcik event
        private void ViewButton_Click(object sender, EventArgs e)
        {
            isSketchMode = false; // Deactivate sketch mode
            ConfigureInteraction();
          
        }

        //Circle Button clcik event
        private void CircleButton_Click(object sender, EventArgs e)
        {
            if (isSketchMode)
            {
                isDrawingCircle = true;  // Set flag to enable drawing circles
                propertiesTextBox.Text = "Drawing Circle"; // Update propertiesTextBox with shape name
                
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
            var Shape = new CircleShape();
            var polyData = Shape.CreateCircle(centerX, centerY, radius);

            // set up the mapper, actor, and renderer
            var mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(polyData.GetProducerPort());

            var actor = vtkActor.New();
            actor.SetMapper(mapper);
            actor.GetProperty().SetColor(0, 0, 0); // Black color


            //new//
            // Add click event handler
            var circleShape = new CircleShape { CenterX = centerX, CenterY = centerY, Radius = radius };
            AddShapeClickHandlers(actor, circleShape);
            //---


            renderer.AddActor(actor);
            renderer.ResetCamera(); // Ensure the circle is visible
            renderWindowControl1.RenderWindow.Render(); // Render the scene
        }


        // Update the circle shape in the renderer
        private void UpdateCircleShape(vtkActor actor, CircleShape circle)
        {
            // Remove the old actor
            renderer.RemoveActor(actor);

            // Create a new circle with the updated radius
            var polyData = circle.CreateCircle(circle.CenterX, circle.CenterY, circle.Radius);
            var mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(polyData.GetProducerPort());

            actor.SetMapper(mapper);
            renderer.AddActor(actor);
            renderWindowControl1.RenderWindow.Render(); // Re-render the scene
        }

        // connector to create Line
        private void DrawLine(double[] worldStartPos, double[] worldEndPos)
        {

            if (isSketchMode)
            {
                var lineShape = new LineShape(); // Instantiate the model
                var polyData = lineShape.CreateLine(worldStartPos, worldEndPos); // Get the line's polyData

                // set up the mapper, actor, and renderer
                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetColor(0, 0, 0); // Black color

        
                // Add click event handler
                var lineShape1 = new LineShape
                {
                    Point1X = worldStartPos[0],
                    Point1Y = worldStartPos[1],
                    Point2X = worldEndPos[0],
                    Point2Y = worldEndPos[1]
                };
                 AddShapeClickHandlers(actor, lineShape1);
            

                renderer.AddActor(actor); // Add the line to the renderer
                renderer.ResetCamera(); // Ensure the line is visible

                renderWindowControl1.RenderWindow.Render(); // Render the updated scene
            }
        }


        // Update the line shape in the renderer
        private void UpdateLineShape(vtkActor actor, LineShape line)
        {
            // Remove the old actor
            renderer.RemoveActor(actor);

            // Create a new line with the updated points
            var polyData = line.CreateLine(new double[] { line.Point1X, line.Point1Y }, new double[] { line.Point2X, line.Point2Y });
            var mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(polyData.GetProducerPort());

            actor.SetMapper(mapper);
            renderer.AddActor(actor);
            renderWindowControl1.RenderWindow.Render(); // Re-render the scene
        }

        // connector to create Point
        private void DrawPoint(double[] worldStartPos)
        {
            if (isSketchMode) // Ensure sketch mode is active
            {
                // Create the point using the provided function
                var pointShape = new PointShape();
                var polyData = pointShape.CreatePoint(worldStartPos);

                // set up the mapper, actor, and renderer
                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                // Create actor for the point
                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetPointSize(100); // Set the point size
                actor.GetProperty().SetColor(0, 0, 0); // Set the point color to black

             
                // Add click event handler
                var pointShape1 = new PointShape { X = worldStartPos[0], Y = worldStartPos[1] };
                AddShapeClickHandlers(actor, pointShape1);
             

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

   
        // Update the point shape in the renderer
        private void UpdatePointShape(vtkActor actor, PointShape point)
        {
            // Remove the old actor
            renderer.RemoveActor(actor);

            // Create a new point with the updated position
            var polyData = point.CreatePoint(new double[] { point.X, point.Y });
            var mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(polyData.GetProducerPort());

            actor.SetMapper(mapper);
            actor.GetProperty().SetPointSize(100); // Set the point size
            actor.GetProperty().SetColor(0, 0, 0); // Set the point color to black

            renderer.AddActor(actor);
            renderWindowControl1.RenderWindow.Render(); // Re-render the scene
        }

        // connector to create arc
        private void DrawArc(double[] worldStartPos, double[] worldEndPos)
        {
            if (isSketchMode)  // Check if sketch mode is active
            {
                var arcShape = new ArcShape(); // Instantiate the model
                var polyData = arcShape.CreateArc(worldStartPos, worldEndPos); // Create the arc with specified parameters

                // set up the mapper, actor, and renderer
                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                // Create a vtkActor to represent the arc in the scene or  data pipeline to the rendering
                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetColor(0, 0, 0); // Set arc color to black

                // Add the actor to the renderer
                renderer.AddActor(actor);
                renderer.ResetCamera(); // Ensure the arc is in view
                renderWindowControl1.RenderWindow.Render(); // Render the updated scene

            }
        }

       
        // Update the arc shape in the renderer
        private void UpdateArcShape(vtkActor actor, ArcShape arc)
        {
            // Remove the old actor
            renderer.RemoveActor(actor);

            // Create a new arc with the updated points
            var mapper = vtkPolyDataMapper.New();
          

            actor.SetMapper(mapper);
            actor.GetProperty().SetColor(0, 0, 0); // Set the arc color to black

            renderer.AddActor(actor);
            renderWindowControl1.RenderWindow.Render(); // Re-render the scene
        }



        // connector to create Ellipse
        private void DrawEllipse(double[] worldStartPos, double[] worldEndPos)
        {
            if (isSketchMode)  // Check if sketch mode is active
            {
                var ellipseShape = new EllipseShape(); // Instantiate the model
                var polyData = ellipseShape.CreateEllipse(worldStartPos, worldEndPos); // Get the polyData for the ellipse

                //set up the mapper, actor, and renderer
                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetColor(0, 0, 0); // Set the ellipse's color to black
             
                // Add click event handler
                var ellipseShape1 = new EllipseShape
                {
                    Point1X = worldStartPos[0],
                    Point1Y = worldStartPos[1],
                    Point2X = worldEndPos[0],
                    Point2Y = worldEndPos[1]
                };
                AddShapeClickHandlers(actor, ellipseShape1);
              

                renderer.AddActor(actor); // Add the ellipse to the renderer
                                          //   renderer.ResetCamera(); // Ensure the ellipse is visible
                renderWindowControl1.RenderWindow.Render(); // Re-render the s
            }
        }

        // Update the ellipse shape in the renderer
        private void UpdateEllipseShape(vtkActor actor, EllipseShape ellipse)
        {
            // Remove the old actor
            renderer.RemoveActor(actor);

            // Create a new ellipse with the updated points
            var polyData = ellipse.CreateEllipse(new double[] { ellipse.Point1X, ellipse.Point1Y }, new double[] { ellipse.Point2X, ellipse.Point2Y });
            var mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(polyData.GetProducerPort());

            actor.SetMapper(mapper);
            actor.GetProperty().SetColor(0, 0, 0); // Set the ellipse color to black

            renderer.AddActor(actor);
            renderWindowControl1.RenderWindow.Render(); // Re-render the scene
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

        // To draw plain 
        private void DrawPlane(string plane)
        {
            
            var mapper = vtkPolyDataMapper.New();
        //    mapper.SetInputConnection(planeSource.GetOutputPort());

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


