// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Sta.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Xaml.Wpf.Development.Tests;

using System;
using System.Runtime.ExceptionServices;
using System.Threading;

/// <summary>
/// Runs a test body on a thread WPF will have: a framework element asks for the input manager as it is built, and
/// that one refuses a thread that is not single threaded apartment, which the test host's threads are not.
/// </summary>
public static class Sta
{
    public static void Run(Action action)
    {
        ExceptionDispatchInfo? exceptionDispatchInfo = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        thread.Join();
        exceptionDispatchInfo?.Throw();
    }
}
