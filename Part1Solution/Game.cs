using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace WindowEngine
{
    // Game class handles the logic and rendering for our 3D graphics app
    public class Game
    {
        // Surface for 2D pixel operations (simplified for this demo)
        private Surface screen;
        // Angle for spinning animation
        private float angle = 0f;

        // Frame counter
        private int frameCount = 0;

        // Constructor: Initialize with screen dimensions
        public Game(int width, int height)
        {
            screen = new Surface(width, height);
        }

        // Initialize OpenGL state and resources
        public void Init()
        {
            // Set clear color to dark blue for a clean background
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            // Enable depth testing for 3D rendering
            GL.Enable(EnableCap.DepthTest);
            // Set up a basic orthographic projection (2D-like for simplicity)
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-10.0, 10.0, -10.0, 10.0, -10.0, 10.0); // World coords: -2 to 2
        }

        // Called each frame to update and render
        public void Tick()
        {
            // Clear the screen and depth buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            screen.Clear(CreateColor(0, 0, 0)); // Clear pixel surface to black

            // Update angle for spinning animation (radians per frame)
            angle += 0.005f;

            //Exercise 1 and 2
            //int squareSize = 300; // Size of the square in pixels
            //int centerX = screen.width / 2;
            //int centerY = screen.height / 2;
            //int startX = (screen.width - squareSize) / 2;
            //int startY = (screen.height - squareSize) / 2;

            //for (int y = 0; y < squareSize; y++)
            //{
            //    for (int x = 0; x < squareSize; x++)
            //    {
            //        int pixelX = startX + x;
            //        int pixelY = startY + y;
            //        if (pixelX >= 0 && pixelX < screen.width && pixelY >= 0 && pixelY < screen.height)
            //        {
            //            int r = (x * 255) / squareSize;
            //            int g = (y * 255) / squareSize;
            //            int b = (int)(128 + 127 * Math.Sin(angle * 0.05)); // Oscillate blue channel

            //            int color = CreateColor(r, g, b);

            //            int location = pixelY * screen.width + pixelX;
            //            screen.pixels[location] = color;
            //        }
            //    }
            //}

            //Exercise 3
            float centerX = screen.width / 2.0f;
            float centerY = screen.height / 2.0f;
            int cubeSize = 100; // Size of the cube in pixels

            // Test zooming by using a smaller world range (e.g., -5 to 5 for zoom-in effect)
            float worldMinX = -5.0f;
            float worldMaxX = 5.0f;
            float worldMinY = -5.0f;
            float worldMaxY = 5.0f;

            //float scale = Math.Min(screen.width, screen.height) / 8.0f; // Scale to fit screen
            float pulse = 1.0f + 0.25f * (float)Math.Sin(angle * 0.01f); // Pulsate between 0.75 and 1.25


            // Define cube corners in 2D space
            float[,] corners = new float[,]
            {
                {-1.0f, -1.0f},
                {1.0f, -1.0f},
                {1.0f, 1.0f},
                {-1.0f, 1.0f}
            };

            float cosA = (float)Math.Cos(angle);
            float sinA = (float)Math.Sin(angle);

            int[,] rotated = new int[4, 2];
            for (int i = 0; i < 4; i++)
            {
                float x = corners[i, 0] * pulse;
                float y = corners[i, 1] * pulse;
                float rotX = x * cosA - y * sinA;
                float rotY = x * sinA + y * cosA;

                rotated[i, 0] = (int)TX(rotX, worldMinX, worldMaxX, centerX, screen.width);
                rotated[i, 1] = (int)TY(rotY, worldMinY, worldMaxY, centerY, screen.height);
            }

            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                screen.DrawLine(rotated[i, 0], rotated[i, 1], rotated[next, 0], rotated[next, 1], CreateColor(255, 255, 255));
            }

            frameCount++;

            // Render in 3D using OpenGL
            screen.RenderGL();
        }

        private int CreateColor(int r, int g, int b)
        {
            return (255 << 24) | (r << 16) | (g << 8) | b; // ARGB format
        }

        //exercise 3
        //private int TX(float wx, int centerX, float scale)
        //{
        //    return (int)(centerX + wx * scale); // World coords -2 to 2 maps to screen
        //}

        //private int TY(float wy, int centerY, float scale)
        //{
        //    return (int)(centerY - wy * scale); // Invert Y for screen coords
        //    //need to invert Y because screen Y goes down in pixels whereas world Y goes up in units,
        //    //and if we don't invert it will spin upside down
        //}

        //exercise 4 - with zooming
        private float TX(float wx, float worldMinX, float worldMaxX, float centerX, float screenWidth)
        {
            float worldCenterX = (worldMinX + worldMaxX) / 2.0f;
            float scaleX = screenWidth / (worldMaxX - worldMinX);
            return centerX + (wx - worldCenterX) * scaleX;
        }

        private float TY(float wy, float worldMinY, float worldMaxY, float centerY, float screenHeight)
        {
            float worldCenterY = (worldMinY + worldMaxY) / 2.0f;
            float scaleY = screenHeight / (worldMaxY - worldMinY);
            return centerY - (wy - worldCenterY) * scaleY;
        }
    }

    // Simple Surface class to mimic pixel array (placeholder for 2D operations)
    public class Surface
    {
        public int[] pixels;
        public int width, height;

        private int textureId;
        private int vao, vbo;
        private int shaderProgram;

        public Surface(int width, int height)
        {
            this.width = width;
            this.height = height;
            pixels = new int[width * height];

            // Generate texture
            textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // Simple shader setup (vertex and fragment shaders)
            string vertexShaderSource = @"
                #version 330 core
                layout(location = 0) in vec2 aPosition;
                layout(location = 1) in vec2 aTexCoord;
                out vec2 TexCoord;
                void main()
                {
                    gl_Position = vec4(aPosition, 0.0, 1.0);
                    TexCoord = aTexCoord;
                }";

            // Fragment shader to display texture
            string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;
                in vec2 TexCoord;
                uniform sampler2D screenTexture;
                void main()
                {
                    FragColor = texture(screenTexture, TexCoord);
                }";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            float[] vertices = {
                // Positions   // TexCoords
                -2.0f,  2.0f,  0.0f, 0.0f, // Top-left
                 2.0f,  2.0f,  1.0f, 0.0f, // Top-right
                 2.0f, -2.0f,  1.0f, 1.0f, // Bottom-right
                -2.0f, -2.0f,  0.0f, 1.0f  // Bottom-left
            };

            // Setup VAO and VBO for a full-screen quad
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            int stride = 4 * sizeof(float);

            // Position attribute
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            // TexCoord attribute
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        // Clear the surface with a specific color
        public void Clear(int color)
        {
            Array.Fill(pixels, color);
        }

        // Bresenham's line algorithm
        public void DrawLine(int x0, int y0, int x1, int y1, int color)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            while (true)
            {
                Plot(x0, y0, color);
                if (x0 == x1 && y0 == y1) break;
                int err2 = err * 2;
                if (err2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (err2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        private void Plot(int x, int y, int color)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                pixels[y * width + x] = color;
            }
        }

        // Custom OpenGL rendering
        public void RenderGL()
        {
            //Update texture with pixel data
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            //render the texture to a full-screen quad
            GL.UseProgram(shaderProgram);
            GL.BindVertexArray(vao);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "screenTexture"), 0);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }
    }
}

