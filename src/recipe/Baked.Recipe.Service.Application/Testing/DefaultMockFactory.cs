﻿using Moq;

namespace Baked.Testing;

public class DefaultMockFactory : IMockFactory
{
    public virtual object Create(IServiceProvider _, MockDescriptor mockDescriptor)
    {
        var mockType = typeof(Mock<>).MakeGenericType(mockDescriptor.Type);
        var mockInstance = (Mock?)Activator.CreateInstance(mockType)
            ?? throw new ArgumentException($"Activator could not create instance of '{mockType}'", nameof(mockDescriptor));

        mockDescriptor.Setup?.Invoke(mockInstance);

        return mockInstance.Object;
    }
}