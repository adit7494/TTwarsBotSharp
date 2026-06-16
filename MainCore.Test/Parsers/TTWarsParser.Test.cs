using MainCore.Parsers;

namespace MainCore.Test.Parsers
{
    /// <summary>
    /// Tests for TTWars-specific parser functionality.
    /// TTWars is a speed version of Travian with different HTML structure.
    /// </summary>
    public class TTWarsParser : BaseParser
    {
        private const string LoginPage = "Parsers/TTWars/LoginPage.html";
        private const string ResourceFields = "Parsers/TTWars/ResourceFields.html";
        private const string Buildings = "Parsers/TTWars/Buildings.html";

        #region LoginParser Tests

        [Theory]
        [InlineData(LoginPage, false)]
        [InlineData(ResourceFields, true)]
        [InlineData(Buildings, true)]
        public void IsIngamePage_TTWars(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.LoginParser.IsIngamePage(_html);
            actual.ShouldBe(expected);
        }

        [Theory]
        [InlineData(LoginPage, true)]
        [InlineData(ResourceFields, false)]
        [InlineData(Buildings, false)]
        public void IsLoginPage_TTWars(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.LoginParser.IsLoginPage(_html);
            actual.ShouldBe(expected);
        }

        [Fact]
        public void GetLoginButton_TTWars()
        {
            _html.Load(LoginPage);
            var actual = MainCore.Parsers.LoginParser.GetLoginButton(_html);
            actual.ShouldNotBeNull();
        }

        [Fact]
        public void GetUsernameInput_TTWars()
        {
            _html.Load(LoginPage);
            var actual = MainCore.Parsers.LoginParser.GetUsernameInput(_html);
            actual.ShouldNotBeNull();
        }

        [Fact]
        public void GetPasswordInput_TTWars()
        {
            _html.Load(LoginPage);
            var actual = MainCore.Parsers.LoginParser.GetPasswordInput(_html);
            actual.ShouldNotBeNull();
        }

        [Fact]
        public void IsTTWarsLoginPage_LoginPage()
        {
            _html.Load(LoginPage);
            var actual = MainCore.Parsers.LoginParser.IsTTWarsLoginPage(_html);
            actual.ShouldBeTrue();
        }

        [Fact]
        public void IsTTWarsLoginPage_ResourceFields()
        {
            _html.Load(ResourceFields);
            var actual = MainCore.Parsers.LoginParser.IsTTWarsLoginPage(_html);
            actual.ShouldBeFalse();
        }

        [Fact]
        public void GetLoginForm_TTWars()
        {
            _html.Load(LoginPage);
            var actual = MainCore.Parsers.LoginParser.GetLoginForm(_html);
            actual.ShouldNotBeNull();
        }

        #endregion

        #region BuildingLayoutParser Tests

        [Fact]
        public void GetFields_TTWars()
        {
            _html.Load(ResourceFields);
            var actual = MainCore.Parsers.BuildingLayoutParser.GetFields(_html).ToList();
            actual.ShouldNotBeEmpty();
            actual.Count.ShouldBe(18); // TTWars has 18 resource fields
        }

        [Fact]
        public void GetFields_TTWars_HasCorrectBuildingTypes()
        {
            _html.Load(ResourceFields);
            var fields = MainCore.Parsers.BuildingLayoutParser.GetFields(_html).ToList();

            // Check that we have the expected building types
            var woodcutters = fields.Where(f => f.Type == MainCore.Enums.BuildingEnums.Woodcutter).Count();
            var clayPits = fields.Where(f => f.Type == MainCore.Enums.BuildingEnums.ClayPit).Count();
            var ironMines = fields.Where(f => f.Type == MainCore.Enums.BuildingEnums.IronMine).Count();
            var croplands = fields.Where(f => f.Type == MainCore.Enums.BuildingEnums.Cropland).Count();

            woodcutters.ShouldBeGreaterThan(0);
            clayPits.ShouldBeGreaterThan(0);
            ironMines.ShouldBeGreaterThan(0);
            croplands.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void GetFields_TTWars_HasCorrectLevels()
        {
            _html.Load(ResourceFields);
            var fields = MainCore.Parsers.BuildingLayoutParser.GetFields(_html).ToList();

            // All fields should have a level > 0
            foreach (var field in fields)
            {
                field.Level.ShouldBeGreaterThan(0);
            }
        }

        [Fact]
        public void GetInfrastructures_TTWars()
        {
            _html.Load(Buildings);
            var actual = MainCore.Parsers.BuildingLayoutParser.GetInfrastructures(_html).ToList();
            actual.ShouldNotBeEmpty();
        }

        [Fact]
        public void GetInfrastructures_TTWars_HasMainBuilding()
        {
            _html.Load(Buildings);
            var buildings = MainCore.Parsers.BuildingLayoutParser.GetInfrastructures(_html).ToList();

            // Should have a Main Building at location 26
            var mainBuilding = buildings.FirstOrDefault(b => b.Location == 26);
            mainBuilding.ShouldNotBeNull();
            mainBuilding.Type.ShouldBe(MainCore.Enums.BuildingEnums.MainBuilding);
        }

        [Fact]
        public void IsDorf1Page_ResourceFields()
        {
            _html.Load(ResourceFields);
            var actual = MainCore.Parsers.BuildingLayoutParser.IsDorf1Page(_html);
            actual.ShouldBeTrue();
        }

        [Fact]
        public void IsDorf1Page_Buildings()
        {
            _html.Load(Buildings);
            var actual = MainCore.Parsers.BuildingLayoutParser.IsDorf1Page(_html);
            actual.ShouldBeFalse();
        }

        [Fact]
        public void IsDorf2Page_Buildings()
        {
            _html.Load(Buildings);
            var actual = MainCore.Parsers.BuildingLayoutParser.IsDorf2Page(_html);
            actual.ShouldBeTrue();
        }

        [Fact]
        public void IsDorf2Page_ResourceFields()
        {
            _html.Load(ResourceFields);
            var actual = MainCore.Parsers.BuildingLayoutParser.IsDorf2Page(_html);
            actual.ShouldBeFalse();
        }

        #endregion

        #region NumberParser Tests

        [Theory]
        [InlineData("00:00:02", 2)]
        [InlineData("00:01:30", 90)]
        [InlineData("01:30:00", 5400)]
        public void ToDuration_StandardFormat(string input, int expectedSeconds)
        {
            var actual = input.ToDuration();
            actual.TotalSeconds.ShouldBe(expectedSeconds);
        }

        [Theory]
        [InlineData("00:00:02 (+332 ms)", 2, 332)]
        [InlineData("00:01:30 (+500 ms)", 90, 500)]
        public void ToDuration_TTWarsFormat_WithMilliseconds(string input, int expectedSeconds, int expectedMilliseconds)
        {
            var actual = input.ToDuration();
            ((int)actual.TotalSeconds).ShouldBe(expectedSeconds);
            actual.Milliseconds.ShouldBe(expectedMilliseconds);
        }

        [Fact]
        public void ToDuration_EmptyString_ReturnsZero()
        {
            var actual = "".ToDuration();
            actual.ShouldBe(TimeSpan.Zero);
        }

        [Fact]
        public void ToDuration_NullString_ReturnsZero()
        {
            var actual = ((string)null).ToDuration();
            actual.ShouldBe(TimeSpan.Zero);
        }

        #endregion

        #region ServerTypeDetector Tests

        [Theory]
        [InlineData("https://nor4.ttwars.com", MainCore.Enums.ServerType.TTWars)]
        [InlineData("https://unl7.ttwars.com", MainCore.Enums.ServerType.TTWars)]
        [InlineData("https://ts1.x1.international.travian.com", MainCore.Enums.ServerType.Travian)]
        [InlineData("https://ts2.x1.international.travian.com", MainCore.Enums.ServerType.Travian)]
        public void DetectFromUrl_ReturnsCorrectServerType(string url, MainCore.Enums.ServerType expected)
        {
            var actual = MainCore.UI.Models.Validators.ServerTypeDetector.DetectFromUrl(url);
            actual.ShouldBe(expected);
        }

        [Fact]
        public void DetectFromUrl_EmptyString_ReturnsTravian()
        {
            var actual = MainCore.UI.Models.Validators.ServerTypeDetector.DetectFromUrl("");
            actual.ShouldBe(MainCore.Enums.ServerType.Travian);
        }

        [Fact]
        public void DetectFromUrl_NullString_ReturnsTravian()
        {
            var actual = MainCore.UI.Models.Validators.ServerTypeDetector.DetectFromUrl(null);
            actual.ShouldBe(MainCore.Enums.ServerType.Travian);
        }

        #endregion
    }
}
