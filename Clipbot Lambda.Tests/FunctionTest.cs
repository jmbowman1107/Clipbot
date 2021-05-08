using Xunit;
using Amazon.Lambda.TestUtilities;

namespace ClipbotLambda.Tests
{
    public class FunctionTest
    {
        #region TestPostToDiscord
        [Fact]
        public async void TestPostToDiscord()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function();
            var context = new TestLambdaContext();
            await function.FunctionHandler(context);
        } 
        #endregion
    }
}