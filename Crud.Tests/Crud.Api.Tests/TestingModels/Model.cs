namespace Crud.Api.Tests.TestingModels
{
    /// <summary>
    /// This is to be used inside this test project only.
    /// Was made public because using private caused _preserver.Setup(m => m.CreateAsync(It.IsAny<Model>())).ReturnsAsync(model); to return null.
    /// </summary>
    public class Model
    {
        public Int32 Id { get; set; }
    }
}
