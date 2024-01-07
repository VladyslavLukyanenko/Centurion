﻿using System.Runtime.Serialization;

namespace Centurion.SeedWork;

public class CoreException : Exception
{
  public CoreException()
  {
  }

  public CoreException(string message)
    : base(message)
  {
  }

  public CoreException(string message, Exception innerException)
    : base(message, innerException)
  {
  }

  protected CoreException(SerializationInfo info, StreamingContext context)
    : base(info, context)
  {
  }
}