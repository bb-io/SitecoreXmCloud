﻿using Apps.Sitecore.Polling;
using Apps.Sitecore.Polling.Memory;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.Sitecore.Base;

namespace Tests.Sitecore
{
    [TestClass]
    public class PollingTests : TestBase
    {
        [TestMethod]
        public async Task OnItemsCreated_IsSuccess()
        {
            var polling = new PollingList(InvocationContext);
            var initialMemory = new DateMemory
            {
                LastInteractionDate = DateTime.UtcNow.AddHours(-1)
            };

            var request = new PollingEventRequest<DateMemory>
            {
                Memory = initialMemory
            };

            var input = new PollingItemRequest
            {
                Locale = "en",
                RootPath = "/sitecore/content/GoTo/LogMeIn/Home"
            };

            var result = await polling.OnItemsCreated(request, input);

            foreach (var item in result.Result.Items)
            {
                Console.WriteLine($"ID: {item.Id}, Name: {item.Name}, Language: {item.Language}, FullPath: {item.FullPath}, CreatedAt: {item.CreatedAt}");
            }
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task OnItemsUpdated_IsSuccess()
        {
            var polling = new PollingList(InvocationContext);
            var initialMemory = new DateMemory
            {
                LastInteractionDate = DateTime.Parse("2025-04-19T15:30:08.0000000Z")
            };

            var request = new PollingEventRequest<DateMemory>
            {
                Memory = initialMemory
            };

            var input = new PollingItemRequest
            {
                Locale = "en",
                RootPath = "/sitecore/content/goto-collection/testpocblackbird/home/pricing-rescue/request-quote/ai/data/form"
            };

            var result = await polling.OnItemsUpdated(request, input);

            //foreach (var item in result.Result.Items)
            //{
            //    Console.WriteLine($"ID: {item.Id}, Name: {item.Name}, Language: {item.Language}, FullPath: {item.FullPath}, CreatedAt: {item.CreatedAt}");
            //}
            Assert.IsNotNull(result);
        }
    }
}
