using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Media.Effect;
using Android.Opengl;
using Android.OS;
using Android.Util;
using Java.Lang;
using Javax.Microedition.Khronos.Opengles;
using WoWonder.Helpers.Utils;
using WoWonder.NiceArt.Models;
using WoWonder.NiceArt.Utils;
using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;
using Exception = Java.Lang.Exception;

namespace WoWonder.NiceArt
{
    public class ImageFilterView : GLSurfaceView, GLSurfaceView.IRenderer
    {
        private readonly int[] MTextures = new int[2];
        private EffectContext MEffectContext;
        private Effect MEffect;
        private readonly TextureRenderer MTexRenderer = new TextureRenderer();
        private int MImageWidth;
        private int MImageHeight;
        private bool MInitialized;
        private PhotoFilter MCurrentEffect;
        private Bitmap MSourceBitmap;
        private CustomEffect MCustomEffect;
        private INiceArt.IOnSaveBitmap MOnSaveBitmap;
        private bool IsSaveImage;
        private Bitmap MFilterBitmap;

        public ImageFilterView(Context context) : base(context)
        {
            try
            {
                Init();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public ImageFilterView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            try
            {
                Init();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public async void Init()
        {
            try
            {
                SetEGLContextClientVersion(2);
                SetRenderer(this);
                RenderMode = Rendermode.WhenDirty;
                await SetFilterEffect(PhotoFilter.None);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public Task SetFilterEffect(PhotoFilter effect)
        {
            try
            {
                MCurrentEffect = effect;
                MCustomEffect = null!;

                RequestRender();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }

            return Task.CompletedTask;
        }

        public Task SetFilterEffect(CustomEffect customEffect)
        {
            try
            {
                MCustomEffect = customEffect;
                RequestRender(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }

            return Task.CompletedTask;
        }

        public void SetSourceBitmap(Bitmap sourceBitmap)
        {
            try
            {
                //if (mSourceBitmap != null && mSourceBitmap.SameAs(sourceBitmap))
                //    mCurrentEffect = PhotoFilter.None;

                MSourceBitmap = sourceBitmap;
                MInitialized = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OnDrawFrame(IGL10 gl)
        {
            try
            { 
                if (!MInitialized)
                {
                    //Only need to do this once
                    MEffectContext = EffectContext.CreateWithCurrentGlContext();
                    MTexRenderer.Init();
                    LoadTextures();
                    MInitialized = true;
                }

                if (MCurrentEffect != PhotoFilter.None || MCustomEffect != null)
                {
                    //if an effect is chosen initialize it and apply it to the texture
                    InitEffect();
                    ApplyEffect();
                }

                RenderResult();

                if (!IsSaveImage) return;

                MFilterBitmap = BitmapUtil.CreateBitmapFromGlSurface(this, gl);

                SetSourceBitmap(MFilterBitmap);

                Console.WriteLine("ImageFilterView", "onDrawFrame: " + MFilterBitmap);

                IsSaveImage = false;
                if (MOnSaveBitmap != null)
                {
                    new Handler(Looper.MainLooper).Post(new Runnable(Run));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void Run()
        {
            try
            {
                MOnSaveBitmap.OnBitmapReady(MFilterBitmap, SaveType.Filter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }


        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
            try
            {
                MTexRenderer?.UpdateViewSize(width, height);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void OnSurfaceCreated(IGL10 gl, EGLConfig config)
        {
            try
            {

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public Task SaveBitmap(INiceArt.IOnSaveBitmap onSaveBitmap)
        {
            try
            {
                MOnSaveBitmap = onSaveBitmap;
                IsSaveImage = true;
                RequestRender();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
            return Task.CompletedTask;
        }

        public void LoadTextures()
        {
            try
            {
                // Generate textures
                GLES20.GlGenTextures(2, MTextures, 0);

                // Load input bitmap
                if (MSourceBitmap != null)
                {
                    MImageWidth = MSourceBitmap.Width;
                    MImageHeight = MSourceBitmap.Height;
                    MTexRenderer.UpdateTextureSize(MImageWidth, MImageHeight);

                    // Upload to texture
                    GLES20.GlBindTexture(GLES20.GlTexture2d, MTextures[0]);
                    GLUtils.TexImage2D(GLES20.GlTexture2d, 0, MSourceBitmap, 0);

                    // Set texture parameters
                    GlToolbox.InitTexParams();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public void InitEffect()
        {
            try
            {
                EffectFactory effectFactory = MEffectContext.Factory;
                MEffect?.Release();

                if (MCustomEffect != null)
                {
                    Dictionary<string, Java.Lang.Object> parameters = MCustomEffect.GetParameters();
                    MEffect = effectFactory.CreateEffect(MCustomEffect.GetEffectName());
                    foreach (var param in parameters)
                    {
                        MEffect.SetParameter(param.Key, param.Value);
                    }
                }
                else
                {
                    // Initialize the correct effect based on the selected menu/action item
                    switch (MCurrentEffect)
                    {
                        case PhotoFilter.AutoFix:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectAutofix);
                            MEffect.SetParameter("scale", 0.5f);
                            break;
                        case PhotoFilter.BlackWhite:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectBlackwhite);
                            MEffect.SetParameter("black", .1f);
                            MEffect.SetParameter("white", .7f);
                            break;
                        case PhotoFilter.Brightness:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectBrightness);
                            MEffect.SetParameter("brightness", 2.0f);
                            break;
                        case PhotoFilter.Contrast:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectContrast);
                            MEffect.SetParameter("contrast", 1.4f);
                            break;
                        case PhotoFilter.CrossProcess:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectCrossprocess);
                            break;
                        case PhotoFilter.Documentary:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectDocumentary);
                            break;
                        case PhotoFilter.DueTone:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectDuotone);
                            MEffect.SetParameter("first_color", Color.Yellow.ToArgb());
                            MEffect.SetParameter("second_color", Color.DarkGray.ToArgb());
                            break;
                        case PhotoFilter.FillLight:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectFilllight);
                            MEffect.SetParameter("strength", .8f);
                            break;
                        case PhotoFilter.FishEye:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectFisheye);
                            MEffect.SetParameter("scale", .5f);
                            break;
                        case PhotoFilter.FlipHorizontal:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectFlip);
                            MEffect.SetParameter("horizontal", true);
                            break;
                        case PhotoFilter.FlipVertical:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectFlip);
                            MEffect.SetParameter("vertical", true);
                            break;
                        case PhotoFilter.Grain:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectGrain);
                            MEffect.SetParameter("strength", 1.0f);
                            break;
                        case PhotoFilter.GrayScale:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectGrayscale);
                            break;
                        case PhotoFilter.Lomoish:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectLomoish);
                            break;
                        case PhotoFilter.Negative:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectNegative);
                            break;
                        case PhotoFilter.None:
                            break;
                        case PhotoFilter.Posterize:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectPosterize);
                            break;
                        case PhotoFilter.Rotate:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectRotate);
                            MEffect.SetParameter("angle", 180);
                            break;
                        case PhotoFilter.Saturate:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectSaturate);
                            MEffect.SetParameter("scale", .5f);
                            break;
                        case PhotoFilter.Sepia:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectSepia);
                            break;
                        case PhotoFilter.Sharpen:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectSharpen);
                            break;
                        case PhotoFilter.Temperature:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectTemperature);
                            MEffect.SetParameter("scale", .9f);
                            break;
                        case PhotoFilter.Tint:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectTint);
                            MEffect.SetParameter("tint", Color.Magenta.ToArgb());
                            break;
                        case PhotoFilter.Vignette:
                            MEffect = effectFactory.CreateEffect(EffectFactory.EffectVignette);
                            MEffect.SetParameter("scale", .5f);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void ApplyEffect()
        {
            try
            {
                MEffect.Apply(MTextures[0], MImageWidth, MImageHeight, MTextures[1]);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void RenderResult()
        {
            try
            {
                if (MCustomEffect != null || MCurrentEffect != PhotoFilter.None)
                {
                    // if no effect is chosen, just render the original bitmap
                    MTexRenderer.RenderTexture(MTextures[1]);
                }
                else
                {
                    // render the result of applyEffect()
                    MTexRenderer.RenderTexture(MTextures[0]);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }
    }
}