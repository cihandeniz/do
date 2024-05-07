﻿using Do.Architecture;
using Do.Authorization;
using Do.ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Do.Test.ExceptionHandling;

public class GeneratingUnauthorizedAccessResponse : TestServiceNfr
{
    protected override Func<AuthorizationConfigurator, IFeature<AuthorizationConfigurator>>? Authorization =>
        c => c.ClaimBased(claims: ["User", "Admin"], baseClaim: "User");

    protected override Func<ExceptionHandlingConfigurator, IFeature<ExceptionHandlingConfigurator>>? ExceptionHandling =>
        c => c.Default(typeUrlFormat: "https://do.mouseless.codes/errors/{0}");

    public override async Task OneTimeTearDown()
    {
        Client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("11111111111111111111111111111111");

        await base.OneTimeTearDown();
    }

    [Test]
    public async Task Authentication_exceptions_are_handled_with_its_own_handler()
    {
        var response = await Client.PostAsync("authorization-samples/require-base-claim", null);

        var problemDetails = response.Content.ReadFromJsonAsync<ProblemDetails>().Result;

        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldBe("Failed to authenticate with given credentials.");
        problemDetails.Status.ShouldBe((int)HttpStatusCode.Unauthorized);
        problemDetails.Title.ShouldBe("Authentication");
        problemDetails.Type.ShouldBe("https://do.mouseless.codes/errors/authentication");
    }

    [Test]
    public async Task Unauthorized_exceptions_are_handled_with_its_own_handler()
    {
        Client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("11111111111111111111111111111111");

        var response = await Client.PostAsync("authorization-samples/require-admin-claim", null);

        var problemDetails = response.Content.ReadFromJsonAsync<ProblemDetails>().Result;

        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldBe("Attempted to perform an unauthorized operation.");
        problemDetails.Status.ShouldBe((int)HttpStatusCode.Forbidden);
        problemDetails.Title.ShouldBe("Unauthorized Access");
        problemDetails.Type.ShouldBe("https://do.mouseless.codes/errors/unauthorized-access");
    }
}