using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDXPractice.System;

namespace SharpDXPractice.Graphics
{
    public class DDX11
    {
        private bool VSyncEnabled { get; set; }
        public int VideoCardMemory { get; set; }
        public string VideoCardDescription { get; set; }
        private SwapChain SwapChain { get; set; }
        public SharpDX.Direct3D11.Device Device { get; private set; }
        public D3D11.DeviceContext DeviceContext { get; private set; }
        private D3D11.RenderTargetView RenderTargetView { get; set; }
        private D3D11.Texture2D DepthStencilBuffer { get; set; }
        public D3D11.DepthStencilState DepthStencilState { get; set; }
        private D3D11.DepthStencilView DepthStencilView { get; set; }
        private D3D11.RasterizerState RasterizerState { get; set; }
        public Matrix ProjectionMatrix { get; set; }
        public Matrix WorldMatrix { get; set; }

        public DDX11()
        {

        }

        public bool Initialize(DSystemConfiguration config, IntPtr windowHandle)
        {
            try
            {
                VSyncEnabled = DSystemConfiguration.VerticalSyncEnabled;

                // Create DX graphics interface factory
                var factory = new Factory1();

                // Create an adapter for the primary graphics interface (video card)
                var adapter = factory.GetAdapter(0);

                // Get the primary adapter output (monitor)
                var monitor = adapter.GetOutput(0);

                // Get modes which fit the R8G8B8A8_UNORM display format for the adapter output (monitor)
                var modes = monitor.GetDisplayModeList(Format.R8G8B8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);

                // Go through all display modes and find the one which matches the screen width and height
                // When a match is found, store the refresh rate for that monitor, if VSync is enabled
                // Otherwise use maximum refresh rate
                var rational = new Rational(0, 1);
                if (VSyncEnabled)
                {
                    foreach (var mode in modes)
                    {
                        if (mode.Width == config.Width && mode.Height == config.Height)
                        {
                            rational = new Rational(mode.RefreshRate.Numerator, mode.RefreshRate.Denominator);
                            break;
                        }
                    }
                }

                // Get the adapter (video card) description
                var adapterDescription = adapter.Description;

                // Store the dedicated video card memory in megabytes
                VideoCardMemory = adapterDescription.DedicatedVideoMemory >> 10 >> 10;

                // Convert the name of the video card to a character array and store it
                VideoCardDescription = adapterDescription.Description.Trim('\0');

                // Release the adapter output
                monitor.Dispose();

                // Release the adapter
                adapter.Dispose();

                //Release the factory
                factory.Dispose();

                // Initialize the swap chain description
                var swapChainDesc = new SwapChainDescription()
                {
                    // Set to a single back buffer
                    BufferCount = 1,
                    // Set width and height of back buffer
                    ModeDescription = new ModeDescription(
                        config.Width, config.Height,
                        rational,
                        Format.R8G8B8A8_UNorm),
                    // Set the usage of the back buffer
                    Usage = Usage.RenderTargetOutput,
                    // Set the handle for the window to render to
                    OutputHandle = windowHandle,
                    // Turn multi-sampling off
                    SampleDescription = new SampleDescription(1, 0),
                    // Set to fullscreen or windowed mode
                    IsWindowed = !DSystemConfiguration.FullScreen,
                    // Don't set advanced flags
                    Flags = SwapChainFlags.None,
                    // Discard back buffer contents after presenting
                    SwapEffect = SwapEffect.Discard
                };

                // Create the swap chain and D3D device
                D3D11.Device device;
                SwapChain swapChain;

                D3D11.Device.CreateWithSwapChain(
                    DriverType.Hardware,
                    D3D11.DeviceCreationFlags.None,
                    swapChainDesc,
                    out device,
                    out swapChain);

                Device = device;
                SwapChain = swapChain;
                DeviceContext = Device.ImmediateContext;

                // Get the pointer to the back buffer
                var backBuffer = D3D11.Texture2D.FromSwapChain<D3D11.Texture2D>(SwapChain, 0);

                // Create the render target view with the back buffer pointer
                RenderTargetView = new D3D11.RenderTargetView(Device, backBuffer);

                // Release ptr to back buffer
                backBuffer.Dispose();

                // Create depth buffer description
                var depthBufferDesc = new D3D11.Texture2DDescription()
                {
                    Width = config.Width,
                    Height = config.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.D24_UNorm_S8_UInt,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = D3D11.ResourceUsage.Default,
                    BindFlags = D3D11.BindFlags.DepthStencil,
                    CpuAccessFlags = 0,
                    OptionFlags = 0
                };

                // Create the depth and stencil buffer using description
                DepthStencilBuffer = new D3D11.Texture2D(Device, depthBufferDesc);

                // Create depth stencil state description; control the type of depth test D3D will do for each pixel
                var depthStencilDesc = new D3D11.DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = D3D11.DepthWriteMask.All,
                    DepthComparison = D3D11.Comparison.Less,
                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,
                    // Stencil operation if pixel front-facing.
                    FrontFace = new D3D11.DepthStencilOperationDescription()
                    {
                        FailOperation = D3D11.StencilOperation.Keep,
                        DepthFailOperation = D3D11.StencilOperation.Increment,
                        PassOperation = D3D11.StencilOperation.Keep,
                        Comparison = D3D11.Comparison.Always
                    },
                    // Stencil operation if pixel is back-facing.
                    BackFace = new D3D11.DepthStencilOperationDescription()
                    {
                        FailOperation = D3D11.StencilOperation.Keep,
                        DepthFailOperation = D3D11.StencilOperation.Decrement,
                        PassOperation = D3D11.StencilOperation.Keep,
                        Comparison = D3D11.Comparison.Always
                    }
                };

                // Create the depth stencil state
                DepthStencilState = new D3D11.DepthStencilState(Device, depthStencilDesc);

                // Set the depth stencil state
                DeviceContext.OutputMerger.SetDepthStencilState(DepthStencilState, 1);

                // Create depth stencil view description; tells D3D to use depth buffer as a depth stencil texture
                var depthStencilViewDesc = new D3D11.DepthStencilViewDescription()
                {
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = D3D11.DepthStencilViewDimension.Texture2D,
                    Texture2D = new D3D11.DepthStencilViewDescription.Texture2DResource()
                    {
                        MipSlice = 0
                    }
                };

                // Create the depth stencil view
                DepthStencilView = new D3D11.DepthStencilView(Device, DepthStencilBuffer, depthStencilViewDesc);

                // Bind the render target view and depth stencil buffer to the output render pipeline
                // Thus, the graphics rendered by the pipeline will be darwn to the back buffer
                // With the graphics written to the back buffer, we can swap it with the front buffer to display them
                DeviceContext.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);

                // Set up the raster description to control how polygons are rendered e.g. make scenes render in wireframe mode
                var rasterDesc = new D3D11.RasterizerStateDescription()
                {
                    IsAntialiasedLineEnabled = false,
                    CullMode = D3D11.CullMode.Back, // Do not draw triangles that are back-facing
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    IsDepthClipEnabled = true,
                    FillMode = D3D11.FillMode.Solid,
                    IsFrontCounterClockwise = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    SlopeScaledDepthBias = 0.0f
                };

                // Create the rasterizer state
                RasterizerState = new D3D11.RasterizerState(Device, rasterDesc);

                // Set the rasterizer state
                DeviceContext.Rasterizer.State = RasterizerState;

                // Set up and create the viewport for rendering
                DeviceContext.Rasterizer.SetViewport(0, 0, config.Width, config.Height, 0, 1);

                // Set up and create projection matrix
                ProjectionMatrix = Matrix.PerspectiveFovLH((float)(Math.PI / 4), ((float)config.Width / (float)config.Height), DSystemConfiguration.ScreenNear, DSystemConfiguration.ScreenDepth);

                // Initialize the world martix to the identity matrix
                WorldMatrix = Matrix.Identity;

                return true; // All operations successful
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Release resources used in the initialize function
        /// </summary>
        public void ShutDown()
        {
            // Set to windowed mode before releasing swap chain
            SwapChain?.SetFullscreenState(false, null);

            RasterizerState?.Dispose();
            RasterizerState = null;
            DepthStencilView?.Dispose();
            DepthStencilView = null;
            DepthStencilState?.Dispose();
            DepthStencilState = null;
            DepthStencilBuffer?.Dispose();
            DepthStencilBuffer = null;
            RenderTargetView?.Dispose();
            RenderTargetView = null;
            SwapChain?.Dispose();
            SwapChain = null;
            Device?.Dispose();
            Device = null;

        }

        /// <summary>
        /// Called whenever we are going to draw a new 3D scene
        /// at the beginning of each frame.
        /// Initializes the buffers to blank and ready to be drawn to.
        /// </summary>
        public void BeginScene(float red, float green, float blue, float alpha)
        {
            // Clear the depth buffer
            DeviceContext.ClearDepthStencilView(DepthStencilView, D3D11.DepthStencilClearFlags.Depth, 1, 0);

            // Clear the back buffer
            DeviceContext.ClearRenderTargetView(RenderTargetView, new Color4(red, green, blue, alpha));
        }

        /// <summary>
        /// Instructs the swap chain to display the 3D scene once
        /// all drawing has completed at the end of each frame.
        /// </summary>
        public void EndScene()
        {
            if (VSyncEnabled)
                SwapChain.Present(1, PresentFlags.None); // Lock to screen's refresh rate
            else
                SwapChain.Present(0, PresentFlags.None); // Present as fast as possible
        } 
    }
}
