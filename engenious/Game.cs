﻿using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using engenious.Graphics;
using engenious.Content;
using System.Linq;
using engenious.Input;

namespace engenious
{
    public delegate void KeyPressDelegate(object sender,char Key);
    public abstract class Game : GraphicsResource
    {
        public event KeyPressDelegate KeyPress;
        public event EventHandler Activated;
        public event EventHandler Deactivated;
        public event EventHandler Exiting;
        public event EventHandler Resized;

        private GameTime gameTime;
        internal int major, minor;
        internal OpenTK.Graphics.GraphicsContextFlags contextFlags;
        private OpenTK.Graphics.IGraphicsContext Context;

        private void ConstructContext()
        {
            OpenTK.Graphics.GraphicsMode mode = OpenTK.Graphics.GraphicsMode.Default;
            OpenTK.Platform.IWindowInfo windowInfo = Window.WindowInfo;
            OpenTK.Graphics.GraphicsContextFlags flags = OpenTK.Graphics.GraphicsContextFlags.Default;
            int major = 1;
            int minor = 0;
            if (this.Context == null || this.Context.IsDisposed)
            {
                OpenTK.Graphics.ColorFormat colorFormat = new OpenTK.Graphics.ColorFormat(8, 8, 8, 8);
                int depth = 24;//TODO: wth?
                int stencil = 0;
                int samples = 0;

                mode = new OpenTK.Graphics.GraphicsMode(colorFormat, depth, stencil, samples);
                try
                {
                    this.Context = new OpenTK.Graphics.GraphicsContext(mode, windowInfo, major, minor, flags);
                }
                catch (Exception ex)
                {
                    major = 1;
                    minor = 0;
                    flags = OpenTK.Graphics.GraphicsContextFlags.Default;
                    this.Context = new OpenTK.Graphics.GraphicsContext(mode, windowInfo, major, minor, flags);
                }
            }
            this.Context.MakeCurrent(windowInfo);
            (this.Context as OpenTK.Graphics.IGraphicsContextInternal).LoadAll();
            ThreadingHelper.Initialize(windowInfo, major, minor, contextFlags);
            this.Context.MakeCurrent(windowInfo);

        }

        public Game()
        {

            OpenTK.Graphics.GraphicsContext.ShareContexts = true;
            Window = new GameWindow(1280, 720);

            ConstructContext();

            GraphicsDevice = new GraphicsDevice(this, Context);
            GraphicsDevice.Viewport = new Viewport(Window.ClientRectangle);
            Window.Context.MakeCurrent(Window.WindowInfo);
            Window.Context.LoadAll();
            GL.Viewport(Window.ClientRectangle);
            //Window.Location = new System.Drawing.Point();
            Mouse = new MouseDevice(Window.Mouse);
            engenious.Input.Mouse.UpdateWindow(Window);
            Window.FocusedChanged += Window_FocusedChanged;
            Window.Closing += delegate(object sender, System.ComponentModel.CancelEventArgs e)
            {
                Exiting?.Invoke(this, new EventArgs());
            };
            
            gameTime = new GameTime(new TimeSpan(), new TimeSpan());

            Window.UpdateFrame += delegate(object sender, FrameEventArgs e)
            {
                Components.Sort();

                gameTime.Update(e.Time);

                Update(gameTime);
            };
            Window.RenderFrame += delegate(object sender, FrameEventArgs e)
            {
                ThreadingHelper.RunUIThread();
                GraphicsDevice.Clear(Color.CornflowerBlue);
                Draw(gameTime);

                GraphicsDevice.Present();
            };
            Window.Resize += delegate(object sender, EventArgs e)
            {
                GraphicsDevice.Viewport = new Viewport(Window.ClientRectangle);
                GL.Viewport(Window.ClientRectangle);

                OnResize(this, e);
            };
            Window.Load += delegate(object sender, EventArgs e)
            {
                Initialize();
                LoadContent();
            };
            Window.Closing += delegate(object sender, System.ComponentModel.CancelEventArgs e)
            {
                OnExiting(this, new EventArgs());
            };
            Window.KeyPress += delegate(object sender, KeyPressEventArgs e)
            {
                KeyPress?.Invoke(this, e.KeyChar);
            };
            //Window.Context.MakeCurrent(Window.WindowInfo);

            Content = new engenious.Content.ContentManager(GraphicsDevice);
            Components = new GameComponentCollection();
            


        }

        void Window_FocusedChanged(object sender, EventArgs e)
        {
            if (Window.Focused)
                Activated?.Invoke(this, e);
            else
                Deactivated?.Invoke(this, e);
        }

        protected virtual void Initialize()
        {
            Window.Visible = true;
            GL.Enable(EnableCap.Blend);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = new DepthStencilState();
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            GraphicsDevice.SamplerStates = new SamplerStateCollection(GraphicsDevice);

            foreach (var component in Components)
            {
                component.Initialize();
            }
        }

        public void Exit()
        {
            Window.Close();
        }

        protected virtual void OnResize(object sender, EventArgs e)
        {
        }

        protected virtual void OnExiting(object sender, EventArgs e)
        {
        }

        public ContentManager Content{ get; private set; }

        
        internal GameWindow Window{ get; private set; }

        public MouseDevice Mouse{ get; private set; }

        public string Title{ get { return Window.Title; } set { Window.Title = value; } }

        public bool IsMouseVisible
        {
            get
            {
                return Window.CursorVisible;
            }
            set
            {
                Window.CursorVisible = value;
            }
        }

        public bool IsActive
        {
            get
            {
                return Window.Focused;
            }
        }

        public void Run()
        {
            Window.Run();
        }

        public void Run(double updatesPerSec, double framesPerSec)
        {
            Window.Run(updatesPerSec, framesPerSec);
        }

        public GameComponentCollection Components{ get; private set; }

        public virtual void LoadContent()
        {
            foreach (var component in Components)
            {
                component.Load();
            }
        }

        public virtual void UnloadContent()
        {
            foreach (var component in Components)
            {
                component.Unload();
            }
        }

        public virtual void Update(GameTime gameTime)
        {

            foreach (var component in Components.Updatables)
            {
                if (!component.Enabled)
                    break;
                component.Update(gameTime);
            }
        }

        public virtual void Draw(GameTime gameTime)
        {
            foreach (var component in Components.Drawables)
            {
                if (!component.Visible)
                    break;
                component.Draw(gameTime);
            }
        }

        #region IDisposable

        public override void Dispose()
        {
            Window.Dispose();

            base.Dispose();
        }

        #endregion
    }
}
