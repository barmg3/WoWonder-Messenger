using Android.Opengl;
using Java.Lang;
using WoWonder.Helpers.Utils;

namespace WoWonder.NiceArt
{
    public class GlToolbox
    {
        public static int LoadShader(int shaderType, string source)
        {
            try
            {
                int shader = GLES20.GlCreateShader(shaderType);
                if (shader != 0)
                {
                    GLES20.GlShaderSource(shader, source);
                    GLES20.GlCompileShader(shader);
                    int[] compiled = new int[1];
                    GLES20.GlGetShaderiv(shader, GLES20.GlCompileStatus, compiled, 0);
                    if (compiled[0] == 0)
                    {
                        string info = GLES20.GlGetShaderInfoLog(shader);
                        GLES20.GlDeleteShader(shader);
                        throw new RuntimeException("Could not compile shader " + shaderType + ":" + info);
                    }
                }
                return shader;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }

        public static int CreateProgram(string vertexSource, string fragmentSource)
        {
            try
            {
                int vertexShader = LoadShader(GLES20.GlVertexShader, vertexSource);
                if (vertexShader == 0)
                {
                    return 0;
                }
                int pixelShader = LoadShader(GLES20.GlFragmentShader, fragmentSource);
                if (pixelShader == 0)
                {
                    return 0;
                }

                int program = GLES20.GlCreateProgram();
                if (program != 0)
                {
                    GLES20.GlAttachShader(program, vertexShader);
                    CheckGlError("glAttachShader");
                    GLES20.GlAttachShader(program, pixelShader);
                    CheckGlError("glAttachShader");
                    GLES20.GlLinkProgram(program);
                    int[] linkStatus = new int[1];
                    GLES20.GlGetProgramiv(program, GLES20.GlLinkStatus, linkStatus, 0);
                    if (linkStatus[0] != GLES20.GlTrue)
                    {
                        string info = GLES20.GlGetProgramInfoLog(program);
                        GLES20.GlDeleteProgram(program);
                        throw new RuntimeException("Could not link program: " + info);
                    }
                }
                return program;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }


        public static void CheckGlError(string op)
        {
            try
            {
                int error = GLES20.GlGetError();
                if (error != GLES20.GlNoError)
                {
                    throw new RuntimeException(op + ": glError " + error);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public static void InitTexParams()
        {
            try
            {
                GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlLinear);
                GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlLinear);
                GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapS, GLES20.GlClampToEdge);
                GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapT, GLES20.GlClampToEdge);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }
    }
}