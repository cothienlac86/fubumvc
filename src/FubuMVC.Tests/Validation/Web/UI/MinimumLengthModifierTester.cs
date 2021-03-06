﻿using FubuMVC.Core.UI.Elements;
using FubuMVC.Core.Validation;
using FubuMVC.Core.Validation.Web.UI;
using NUnit.Framework;
using Shouldly;

namespace FubuMVC.Tests.Validation.Web.UI
{
    [TestFixture]
    public class MinimumLengthModifierTester : ValidationElementModifierContext<MinimumLengthModifier>
    {
        [Test]
        public void adds_the_min_length_data_attribute_for_minimum_length_rule()
        {
            var theRequest = ElementRequest.For(new TargetWithMinLength(), x => x.Value);
            tagFor(theRequest).Data("minlength").ShouldBe(10);
        }

        [Test]
        public void no_data_attribute_when_rule_does_not_exist()
        {
            var theRequest = ElementRequest.For(new TargetWithNoMinLength(), x => x.Value);
            tagFor(theRequest).Data("minlength").ShouldBeNull();
        }


        public class TargetWithMinLength
        {
            [MinimumStringLength(10)]
            public string Value { get; set; }
        }

        public class TargetWithNoMinLength
        {
            public string Value { get; set; }
        }
    }
}