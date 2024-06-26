﻿using Baked.Test.Orm;

namespace Baked.Test.DataAccess;

public class MappingProperties : TestServiceSpec
{
    [Test]
    public async Task Unique()
    {
        var entity = GiveMe.An<Entity>().With(unique: "eb8dd0a1");
        entity.Unique.ShouldBe("eb8dd0a1");

        await entity.Update(unique: "ab8dd0a1");
        entity.Unique.ShouldBe("ab8dd0a1");

        var actual = GiveMe.The<Entities>().By(unique: "ab8dd0a1").FirstOrDefault();
        actual.ShouldBe(entity);
    }

    [Test]
    public void Unique_must_be_unique()
    {
        var entity = GiveMe.An<Entity>().With(unique: "eb8dd0a1");
        entity.Unique.ShouldBe("eb8dd0a1");

        Func<Entity> task = () => GiveMe.An<Entity>().With(unique: "eb8dd0a1");

        task.ShouldThrow<MustBeUniqueException>();
    }

    [Test]
    public async Task Guid()
    {
        var entity = GiveMe.An<Entity>().With(guid: GiveMe.AGuid("eb8dd0a1"));
        entity.Guid.ShouldBe(GiveMe.AGuid("eb8dd0a1"));

        await entity.Update(guid: GiveMe.AGuid("ab8dd0a1"));
        entity.Guid.ShouldBe(GiveMe.AGuid("ab8dd0a1"));

        var actual = GiveMe.The<Entities>().By(guid: GiveMe.AGuid("ab8dd0a1")).FirstOrDefault();
        actual.ShouldBe(entity);
    }

    [Test]
    public async Task String()
    {
        var entity = GiveMe.An<Entity>().With(@string: "string");
        entity.String.ShouldBe("string");

        await entity.Update(@string: "test");
        entity.String.ShouldBe("test");

        var actual = GiveMe.The<Entities>().By(@string: "test").FirstOrDefault();
        actual.ShouldBe(entity);
    }

    [Test]
    public async Task String_data()
    {
        var entity = GiveMe.An<Entity>().With(stringData: "string");
        entity.StringData.ShouldBe("string");

        await entity.Update(stringData: "test");
        entity.StringData.ShouldBe("test");

        var actual = GiveMe.The<Entities>().By(stringData: "test").FirstOrDefault();
        actual.ShouldBe(entity);
    }

    [Test]
    public async Task Object()
    {
        var entity = GiveMe.An<Entity>().With(dynamic: new { dynamic = "dynamic" });
        entity.Dynamic.ShouldBe(new { dynamic = "dynamic" });

        await entity.Update(dynamic: new { update = "update" });
        entity.Dynamic.ShouldBe(new { update = "update" });
    }

    [Test]
    public async Task Int()
    {
        var entity = GiveMe.An<Entity>().With(int32: 5);
        entity.Int32.ShouldBe(5);

        await entity.Update(int32: 1);
        entity.Int32.ShouldBe(1);

        var actual = GiveMe.The<Entities>().By(int32: 1).FirstOrDefault();
        actual.ShouldBe(entity);
    }

    [Test]
    public async Task Enum()
    {
        var entity = GiveMe.An<Entity>().With(@enum: Enumeration.Member1);
        entity.Enum.ShouldBe(Enumeration.Member1);

        await entity.Update(@enum: Enumeration.Member2);
        entity.Enum.ShouldBe(Enumeration.Member2);

        var actual = GiveMe.The<Entities>().By(@enum: Enumeration.Member2).FirstOrDefault();
        actual.ShouldBe(entity);
    }

    [Test]
    public async Task DateTime()
    {
        var entity = GiveMe.An<Entity>().With(dateTime: GiveMe.ADateTime(year: 2023, month: 11, day: 29));
        entity.DateTime.ShouldBe(GiveMe.ADateTime(year: 2023, month: 11, day: 29));

        await entity.Update(dateTime: GiveMe.ADateTime(year: 2023, month: 11, day: 30));
        entity.DateTime.ShouldBe(GiveMe.ADateTime(year: 2023, month: 11, day: 30));

        var actual = GiveMe.The<Entities>().By(dateTime: GiveMe.ADateTime(year: 2023, month: 11, day: 30)).FirstOrDefault();
        actual.ShouldBe(entity);
    }
}