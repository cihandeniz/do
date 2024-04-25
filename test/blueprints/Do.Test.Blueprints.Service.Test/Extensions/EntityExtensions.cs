using Do.Test.Orm;
using Do.Testing;

namespace Do.Test;

public static class EntityExtensions
{
    public static Entity AnEntity(this Stubber giveMe,
       Guid? guid = default,
       string? @string = default,
       string? stringData = default,
       int? int32 = default,
       string? unique = default,
       Uri? uri = default,
       object? @dynamic = default,
       Status? @enum = default,
       DateTime? dateTime = default,
       bool setNowForDateTime = false
    )
    {
        guid ??= giveMe.AGuid();
        @string ??= string.Empty;
        stringData ??= string.Empty;
        int32 ??= 0;
        unique ??= $"giveMe.AGuid()";
        uri ??= giveMe.AUrl();
        dynamic ??= new { };
        @enum ??= Status.Disabled;
        dateTime ??= setNowForDateTime ? giveMe.The<TimeProvider>().GetNow() : giveMe.ADateTime();

        return giveMe.An<Entity>().With(
            guid: guid,
            @string: @string,
            stringData: stringData,
            int32: int32,
            unique: unique,
            uri: uri,
            dynamic: dynamic,
            @enum: @enum,
            dateTime: dateTime
         );
    }
}