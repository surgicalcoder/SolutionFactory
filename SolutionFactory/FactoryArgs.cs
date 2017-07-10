using PowerArgs;

namespace SolutionFactory
{
    public class FactoryArgs
    {
        [ArgShortcut("p")]
        [ArgDescription("Path to Solution FolderRoot")]
        [ArgExistingDirectory]
        [ArgPosition(0)]
        [ArgRequired(PromptIfMissing = false)]
        [ArgExample(@"""c:\path\to\solution""", "Full path to the root of the solution")]
        public string PathToSolution { get; set; }

        [ArgShortcut("n")]
        [ArgDescription("Namespace of new solution")]
        [ArgPosition(1)]
        [ArgRequired(PromptIfMissing = false)]
        public string Namespace { get; set; }

        [ArgShortcut("f")]
        [ArgDescription("Friendly name of solution, with no dot's or spaces.")]
        [ArgPosition(2)]
        [ArgRequired(PromptIfMissing = false)]
        [ArgRegex("^[a-zA-Z0-9_]+$", "Only letters, and underscore!")]
        public string FriendlyName { get; set; }


        [ArgShortcut("c")]
        [ArgPosition(3)]
        public bool CleanOnly { get; set; }
    }
}