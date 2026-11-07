// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Uno.Development.Tester;

using System;
using global::Uno.UI.Hosting;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Said before the host is built, because a window with a backdrop is see through and this head's OpenGL
        // path never clears a surface that is: everything drawn over such a window stays on it, a closed combo box
        // leaving the list it dropped behind. Vulkan draws on the graphics card the same as OpenGL does and clears
        // the surface properly; turning OpenGL off instead leaves the drawing to the processor.
        global::Uno.UI.FeatureConfiguration.Rendering.UseVulkanOnWin32 = true;

        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseX11()
            .UseLinuxFrameBuffer()
            .UseMacOS()
            .UseWin32()
            .Build();

        host.Run();
    }
}
