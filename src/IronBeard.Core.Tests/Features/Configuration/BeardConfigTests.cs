using IronBeard.Core.Features.Configuration;

namespace IronBeard.Core.Tests.Features.Configuration;

public class BeardConfigTests
{
    [Fact]
    public void DefaultSiteTitle_IsMyIronBeardSite()
    {
        var config = new BeardConfig();
        Assert.Equal("My IronBeard Site", config.SiteTitle);
    }

    [Fact]
    public void DefaultIndexFileName_IsIndex()
    {
        var config = new BeardConfig();
        Assert.Equal("Index", config.IndexFileName);
    }

    [Fact]
    public void DefaultLayoutFileName_IsLayout()
    {
        var config = new BeardConfig();
        Assert.Equal("_Layout", config.LayoutFileName);
    }

    [Fact]
    public void DefaultStaticExtensionIgnoreList_ContainsExpectedExtensions()
    {
        var config = new BeardConfig();
        Assert.Contains(".cshtml", config.StaticExtensionIgnoreList);
        Assert.Contains(".md", config.StaticExtensionIgnoreList);
        Assert.Contains(".DS_Store", config.StaticExtensionIgnoreList);
        Assert.Contains(".json", config.StaticExtensionIgnoreList);
        Assert.Equal(4, config.StaticExtensionIgnoreList.Count);
    }

    [Fact]
    public void DefaultExcludeHtmlExtension_IsTrue()
    {
        var config = new BeardConfig();
        Assert.True(config.ExcludeHtmlExtension);
    }

    [Fact]
    public void DefaultEnableMarkdownExtensions_IsFalse()
    {
        var config = new BeardConfig();
        Assert.False(config.EnableMarkdownExtensions);
    }

    [Fact]
    public void Properties_AreMutable()
    {
        var config = new BeardConfig
        {
            SiteTitle = "Test Site",
            IndexFileName = "default",
            LayoutFileName = "_Main",
            ExcludeHtmlExtension = false,
            EnableMarkdownExtensions = true
        };

        Assert.Equal("Test Site", config.SiteTitle);
        Assert.Equal("default", config.IndexFileName);
        Assert.Equal("_Main", config.LayoutFileName);
        Assert.False(config.ExcludeHtmlExtension);
        Assert.True(config.EnableMarkdownExtensions);
    }
}
