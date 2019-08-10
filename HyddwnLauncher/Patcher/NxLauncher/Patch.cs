using System;

namespace HyddwnLauncher.Patcher.NxLauncher
{
    public class Patch
    {
        public FileDownloadInfo FileDownloadInfo { get; protected set; }
        public PatchReason PatchReason { get; protected set; }

        public Patch(FileDownloadInfo fileDownloadInfo, PatchReason patchReason = PatchReason.Older)
        {
            FileDownloadInfo = fileDownloadInfo ?? throw new ArgumentNullException(nameof(fileDownloadInfo));
            PatchReason = patchReason;
        }
    }
}
