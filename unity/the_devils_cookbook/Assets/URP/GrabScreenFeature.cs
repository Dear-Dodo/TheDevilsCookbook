using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class GrabScreenFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public string TextureName = "_GrabPassTransparent";
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    class GrabRenderPass : ScriptableRenderPass
    {
        RenderTargetHandle _TempColorTarget;
        string _TextureName;

        RenderTargetIdentifier _CameraTarget;

        public GrabRenderPass(Settings settings)
        {
            _TextureName = settings.TextureName;
            renderPassEvent = settings.renderPassEvent;
            _TempColorTarget.Init(_TextureName);
        }

        public void Setup(RenderTargetIdentifier cameraTarget)
        {
            _CameraTarget = cameraTarget;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(_TempColorTarget.id, cameraTextureDescriptor);
            cmd.SetGlobalTexture(_TextureName, _TempColorTarget.Identifier());
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            Blit(cmd, _CameraTarget, _TempColorTarget.Identifier());

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_TempColorTarget.id);
        }
    }

    GrabRenderPass GrabPass;

    [SerializeField]
    private Settings _Settings;
    public override void Create()
    {
        GrabPass = new GrabRenderPass(_Settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        GrabPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(GrabPass);
    }
}