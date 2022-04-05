using System;
using Android.Opengl;
using Java.Nio;
using WoWonder.Helpers.Utils;

namespace WoWonder.NiceArt
{
    public class TextureRenderer
    {
        public int MProgram;
        public int MTexSamplerHandle;
        public int MTexCoordHandle;
        public int MPosCoordHandle;

        public FloatBuffer MTexVertices;
        public FloatBuffer MPosVertices;

        public int MViewWidth;
        public int MViewHeight;

        public int MTexWidth;
        public int MTexHeight;

        public static readonly string VertexShader =
            "attribute vec4 a_position;\n" +
            "attribute vec2 a_texcoord;\n" +
            "varying vec2 v_texcoord;\n" +
            "void main() {\n" +
            "  gl_Position = a_position;\n" +
            "  v_texcoord = a_texcoord;\n" +
            "}\n";

        public static readonly string FragmentShader =
            "precision mediump float;\n" +
            "uniform sampler2D tex_sampler;\n" +
            "varying vec2 v_texcoord;\n" +
            "void main() {\n" +
            "  gl_FragColor = texture2D(tex_sampler, v_texcoord);\n" +
            "}\n";

        public static readonly float[] TexVertices =
        {
            0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f
        };

        public static readonly float[] PosVertices =
        {
            -1.0f, -1.0f, 1.0f, -1.0f, -1.0f, 1.0f, 1.0f, 1.0f
        };

        public static readonly int FloatSizeBytes = 4;

        public void Init()
        {
            try
            {
                // Create program
                MProgram = GlToolbox.CreateProgram(VertexShader, FragmentShader);

                // Bind attributes and uniforms
                MTexSamplerHandle = GLES20.GlGetUniformLocation(MProgram, "tex_sampler");
                MTexCoordHandle = GLES20.GlGetAttribLocation(MProgram, "a_texcoord");
                MPosCoordHandle = GLES20.GlGetAttribLocation(MProgram, "a_position");

                // Setup coordinate buffers
                MTexVertices = ByteBuffer.AllocateDirect(TexVertices.Length * FloatSizeBytes).Order(ByteOrder.NativeOrder()).AsFloatBuffer();
                MTexVertices.Put(TexVertices).Position(0);
                MPosVertices = ByteBuffer.AllocateDirect(PosVertices.Length * FloatSizeBytes).Order(ByteOrder.NativeOrder()).AsFloatBuffer();
                MPosVertices.Put(PosVertices).Position(0);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }


        public void TearDown()
        {
            try
            {
                GLES20.GlDeleteProgram(MProgram);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void UpdateTextureSize(int texWidth, int texHeight)
        {
            try
            {
                MTexWidth = texWidth;
                MTexHeight = texHeight;
                ComputeOutputVertices();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void UpdateViewSize(int viewWidth, int viewHeight)
        {
            try
            {
                MViewWidth = viewWidth;
                MViewHeight = viewHeight;
                ComputeOutputVertices();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void RenderTexture(int texId)
        {
            try
            {
                // Bind default FBO
                GLES20.GlBindFramebuffer(GLES20.GlFramebuffer, 0);

                // Use our shader program
                GLES20.GlUseProgram(MProgram);
                GlToolbox.CheckGlError("glUseProgram");

                // Set viewport
                GLES20.GlViewport(0, 0, MViewWidth, MViewHeight);
                GlToolbox.CheckGlError("glViewport");

                // Disable blending
                GLES20.GlDisable(GLES20.GlBlend);

                // Set the vertex attributes
                GLES20.GlVertexAttribPointer(MTexCoordHandle, 2, GLES20.GlFloat, false, 0, MTexVertices);
                GLES20.GlEnableVertexAttribArray(MTexCoordHandle);
                GLES20.GlVertexAttribPointer(MPosCoordHandle, 2, GLES20.GlFloat, false, 0, MPosVertices);
                GLES20.GlEnableVertexAttribArray(MPosCoordHandle);
                GlToolbox.CheckGlError("vertex attribute setup");

                // Set the input texture
                GLES20.GlActiveTexture(GLES20.GlTexture0);
                GlToolbox.CheckGlError("glActiveTexture");
                GLES20.GlBindTexture(GLES20.GlTexture2d, texId);
                GlToolbox.CheckGlError("glBindTexture");
                GLES20.GlUniform1i(MTexSamplerHandle, 0);

                // Draw
                GLES20.GlClearColor(0.0f, 0.0f, 0.0f, 1.0f);
                GLES20.GlClear(GLES20.GlColorBufferBit);
                GLES20.GlDrawArrays(GLES20.GlTriangleStrip, 0, 4);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void ComputeOutputVertices()
        {
            try
            {
                if (MPosVertices != null)
                {
                    float imgAspectRatio = MTexWidth / (float)MTexHeight;
                    float viewAspectRatio = MViewWidth / (float)MViewHeight;
                    float relativeAspectRatio = viewAspectRatio / imgAspectRatio;
                    float x0, y0, x1, y1;
                    if (relativeAspectRatio > 1.0f)
                    {
                        x0 = -1.0f / relativeAspectRatio;
                        y0 = -1.0f;
                        x1 = 1.0f / relativeAspectRatio;
                        y1 = 1.0f;
                    }
                    else
                    {
                        x0 = -1.0f;
                        y0 = -relativeAspectRatio;
                        x1 = 1.0f;
                        y1 = relativeAspectRatio;
                    }
                    float[] coords = new[] { x0, y0, x1, y0, x0, y1, x1, y1 };
                    MPosVertices.Put(coords).Position(0);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }
    }
}