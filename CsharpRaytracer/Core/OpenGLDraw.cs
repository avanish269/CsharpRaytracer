using OpenTK.Graphics.OpenGL4;
using System;

namespace CsharpRaytracer.Core
{
    public class OpenGLDraw
    {
        private int width, height;
        private int fbo;

        public void Init(int W, int H)
        {
            int texture;
            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            this.width = W;
            this.height = H;
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, W, H, 0,
                PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

#pragma warning disable S125 // Sections of code should not be commented out
            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.GenFramebuffers(1, out this.fbo);
#pragma warning restore S125 // Sections of code should not be commented out
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, this.fbo);
            GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
        }

        public void Draw(byte[] image, int W = 0, int H = 0, int atx = 0, int aty = 0)
        {
            if (H == 0) H = this.height;
            if (W == 0) W = this.width;

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, atx, aty, W, H, PixelFormat.Rgb, PixelType.UnsignedByte, image);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, this.fbo);
            GL.BlitFramebuffer(0, 0, W, H, atx, aty, W, H,
                ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
        }
    }
}
