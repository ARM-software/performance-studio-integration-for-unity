/**
 * SPDX-License-Identifier: BSD-3-Clause
 *
 * Copyright (c) 2021, Arm Limited
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * 3. Neither the name of the copyright holder nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Linq;
using System.Collections.Generic;
using NiceIO;
using Bee.Core;
using Bee.NativeProgramSupport;
using Bee.Toolchain.Android;

class BuildProgram
{
    static void Main()
    {
        if(String.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT")))
        {
            throw new ArgumentException("ANDROID_NDK_ROOT is not set, please set it to the path of your NDK.");
        }

        // Target platforms
        List<BuildCommand> android = new List<BuildCommand>();

        // Update these paths to NDK location
        android.Add(BuildCommand.Create(new AndroidNdkToolchain(new AndroidNdkR19(
            Architecture.Armv7, Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT"), false)), "android", "armeabi-v7a"));
        android.Add(BuildCommand.Create(new AndroidNdkToolchain(new AndroidNdkR19(
            Architecture.Arm64, Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT"), false)), "android", "arm64-v8a"));

        NativeProgram pluginProgram = new NativeProgram("MobileStudio");
        pluginProgram.Sources.Add("./streamline_annotate.c");
        pluginProgram.Exceptions.Set(true);
        pluginProgram.RTTI.Set(true);
        pluginProgram.Libraries.Add(new SystemLibrary("log"));
        ProcessProgram(pluginProgram, "mobilestudio", android);
    }

    private static void ProcessProgram(NativeProgram plugin, string targetDir, List<BuildCommand> commands)
    {
        foreach (var command in commands)
        {
            var toolchain = command.ToolChain;
            var config = new NativeProgramConfiguration(CodeGen.Release, toolchain, false);
            var builtProgram = plugin.SetupSpecificConfiguration(config, toolchain.DynamicLibraryFormat);
            var artefact = builtProgram.Path;
            if (command.PostProcess != null)
                artefact = command.PostProcess(artefact, toolchain, targetDir, command.PluginSubFolder);
            Backend.Current.AddAliasDependency(command.Alias, artefact);
        }
    }

    class BuildCommand
    {
        public ToolChain ToolChain;
        public string Alias;
        public string PluginSubFolder;
        public Func<NPath, ToolChain, string, string, NPath> PostProcess = PostProcessDefault;
        public static BuildCommand Create (ToolChain chain, string alias, string pluginSubFolder = "")
        {
            return new BuildCommand() { Alias = alias, ToolChain = chain, PluginSubFolder = pluginSubFolder };
        }
    }

    private static NPath PostProcessDefault(NPath builtProgram, ToolChain toolchain, string pluginDir, string subFolderDir)
    {
        return Copy(builtProgram, builtProgram, toolchain, pluginDir, subFolderDir);
    }

    private static NPath Copy(NPath from, NPath to, ToolChain toolchain, string pluginDir, string subFolderDir)
    {
        string fileName = "mobilestudio";
        if ((toolchain.Platform is LinuxPlatform) || (toolchain.Platform is AndroidPlatform))
            fileName = "libmobilestudio";
        to = new NPath($"../Plugins/{subFolderDir}/{fileName}.{to.Extension}");
        CopyTool.Instance().Setup(to, from);
        return to;
    }
}