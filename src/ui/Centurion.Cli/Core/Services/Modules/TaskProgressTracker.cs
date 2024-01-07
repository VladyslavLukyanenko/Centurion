using System.Reactive.Subjects;
using Grpc.Core;

namespace Centurion.Cli.Core.Services.Modules;

public static class TaskProgressTracker
{
  public static async ValueTask<T> TrackProgress<T>(this ValueTask<T> self, ISubject<bool> tracker)
  {
    tracker.OnNext(true);
    try
    {
      return await self;
    }
    finally
    {
      tracker.OnNext(false);
    }
  }
  public static async ValueTask TrackProgress(this ValueTask self, ISubject<bool> tracker)
  {
    tracker.OnNext(true);
    try
    {
      await self;
    }
    finally
    {
      tracker.OnNext(false);
    }
  }
  public static async Task<T> TrackProgress<T>(this Task<T> self, ISubject<bool> tracker)
  {
    tracker.OnNext(true);
    try
    {
      return await self;
    }
    finally
    {
      tracker.OnNext(false);
    }
  }
  public static async Task TrackProgress(this Task self, ISubject<bool> tracker)
  {
    tracker.OnNext(true);
    try
    {
      await self;
    }
    finally
    {
      tracker.OnNext(false);
    }
  }
  public static async Task<T> TrackProgress<T>(this AsyncUnaryCall<T> self, ISubject<bool> tracker)
  {
    tracker.OnNext(true);
    try
    {
      return await self;
    }
    finally
    {
      tracker.OnNext(false);
    }
  }
}