namespace GenericQuest.Tests
{
    /// <summary>
    /// Simple console test runner. Can be invoked from unit test framework or as standalone.
    /// </summary>
    public static class TestRunner
    {
        public static void Main()
        {
            QuestSystemTests.RunAllTests();
        }
    }
}
