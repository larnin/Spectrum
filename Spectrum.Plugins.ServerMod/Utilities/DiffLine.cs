using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Plugins.ServerMod.Utilities
{
    public class DiffLine
    {
        public string Original;
        public List<string> New;
        public bool Remove = false;
        public DiffLine()
        {
            Original = string.Empty;
            New = new List<string>();
        }
        public DiffLine(string original)
        {
            Original = original;
            New = new List<string>();
        }

        public static List<DiffLine> GetDiffLines(IEnumerable<string> lines)
        {
            List<DiffLine> personalDiff = new List<DiffLine>();
            foreach (string line in lines)
            {
                personalDiff.Add(new DiffLine(line));
            }
            var finalLine = new DiffLine();
            finalLine.Remove = true;
            personalDiff.Add(finalLine);
            var topLine = new DiffLine();
            topLine.Remove = true;
            personalDiff.Insert(0, topLine);
            return personalDiff;
        }

        public static void ExecuteDiff(List<DiffLine> personalDiff, IEnumerable<string> linesInput)
        {
            ExecuteDiff(personalDiff, linesInput, DiffOptions.None);
        }

        public class DiffBranch
        {
            public DiffBranch parent = null;
            public int diffIndexStart = 0;
            public int diffIndexEnd = 0;
            public int inputIndexStart = 0;
            public int inputIndexEnd = 0;
            public int diffIndexNext = 0;
            public int inputIndexNext = 0;
            public int cumulativeDeletions = 0;
            public int cumulativeAdds = 0;
            public int cumulativeFounds = 0;
            public int deletions { get { return parent == null ? cumulativeDeletions : cumulativeDeletions - parent.cumulativeDeletions; } }
            public int adds { get { return parent == null ? cumulativeAdds : cumulativeAdds - parent.cumulativeAdds; } }
            public int founds { get { return parent == null ? cumulativeFounds : cumulativeFounds - parent.cumulativeFounds; } }
            public DiffBranch() { }
            List<DiffBranch> getFullBranch()
            {
                List<DiffBranch> branchList = new List<DiffBranch>() { this };
                while (branchList[0].parent != null)
                    branchList.Insert(0, branchList[0].parent);
                return branchList;
            }
            public void execute(List<DiffLine> diffLines, List<string> inputBase, DiffOptions options)
            {
                bool addFirst = (options & DiffOptions.AddFirst) != 0;
                List<string> inputLines = new List<string>(inputBase);
                inputLines.Insert(0, string.Empty);
                inputLines.Add(string.Empty);
                List<DiffBranch> fromAncestor = getFullBranch();
                int lastAddLine = -1;
                int insertAt = 0;
                foreach (DiffBranch change in fromAncestor)
                {
                    for (int diffIndex = change.diffIndexStart; diffIndex < change.diffIndexEnd; diffIndex++)
                    {
                        diffLines[diffIndex].Remove = true;
                    }
                    if (change.diffIndexEnd != lastAddLine)
                    {
                        lastAddLine = change.diffIndexEnd;
                        insertAt = 0;
                    }
                    for (int inputIndex = change.inputIndexStart; inputIndex < Math.Min(change.inputIndexEnd, inputLines.Count - 1); inputIndex++)
                    {
                        if (addFirst)
                            diffLines[change.diffIndexEnd].New.Insert(insertAt, inputLines[inputIndex]);
                        else
                            diffLines[change.diffIndexEnd].New.Add(inputLines[inputIndex]);
                        insertAt++;
                    }
                }
            }
            public bool BetterThan(DiffBranch branch)
            {
                return branch == null
                    || cumulativeDeletions + cumulativeAdds < branch.cumulativeDeletions + branch.cumulativeAdds;
                    /*
                    || cumulativeDeletions < branch.cumulativeDeletions
                    || (cumulativeDeletions == branch.cumulativeDeletions
                        && cumulativeAdds < branch.cumulativeAdds);
                //*/
            }
        }

        public static DiffBranch GetBestDiffBranch(List<DiffLine> diffBase, IEnumerable<string> inputBase)
        {
            List<DiffLine> diffLines = new List<DiffLine>(diffBase);
            List<string> inputLines = new List<string>(inputBase);
            inputLines.Insert(0, string.Empty);
            inputLines.Add(string.Empty);
            DiffBranch bestBranch = null;
            List<DiffBranch> currentBranches = new List<DiffBranch>();
            currentBranches.Add(new DiffBranch
            {
                diffIndexStart = 0,
                diffIndexEnd = 0,
                inputIndexStart = 0,
                inputIndexEnd = 0,
                cumulativeFounds = 1,
                diffIndexNext = 1,
                inputIndexNext = 1,
            });
            int count = 0;
            while (currentBranches.Count > 0)
            {
                //const int branchIndex = 0;
                for (int branchIndex = currentBranches.Count - 1; branchIndex >= 0; branchIndex--)
                {
                    count++;
                    DiffBranch currentBranch = currentBranches[branchIndex];
                    currentBranches.RemoveAt(branchIndex);
                    if (!currentBranch.BetterThan(bestBranch))
                        continue;
                    int inputIndex = currentBranch.inputIndexNext;
                    int diffIndexStart = currentBranch.diffIndexNext;
                    bool foundInstantly = false;
                    string input = inputLines[inputIndex];
                    for (int diffIndex = diffIndexStart; diffIndex < diffLines.Count; diffIndex++)
                    {
                        DiffLine diff = diffLines[diffIndex];
                        if (input == diff.Original)
                        {
                            foundInstantly = foundInstantly || diffIndex == diffIndexStart;
                            int foundIndex = 1;
                            for (; foundIndex < Math.Min(diffLines.Count - diffIndex, inputLines.Count - inputIndex); foundIndex++)
                                if (inputLines[inputIndex + foundIndex] != diffLines[diffIndex + foundIndex].Original)
                                    break;
                            bool isToEnd = diffIndex == diffLines.Count - 1;
                            var newBranch = new DiffBranch
                            {
                                parent = currentBranch,
                                diffIndexStart = diffIndexStart,
                                diffIndexEnd = diffIndex,
                                inputIndexStart = inputIndex,
                                inputIndexEnd = isToEnd ? inputLines.Count - 1 : inputIndex,
                                cumulativeDeletions = currentBranch.cumulativeDeletions + (diffIndex - diffIndexStart),
                                cumulativeAdds = currentBranch.cumulativeAdds,
                                cumulativeFounds = currentBranch.cumulativeFounds + foundIndex,
                                diffIndexNext = diffIndex + foundIndex,
                                inputIndexNext = isToEnd ? inputLines.Count : inputIndex + foundIndex,
                            };
                            newBranch.cumulativeAdds += (newBranch.inputIndexEnd - newBranch.inputIndexStart);
                            if (newBranch.BetterThan(bestBranch))
                            {
                                if (newBranch.inputIndexNext == inputLines.Count)
                                    bestBranch = newBranch;
                                else
                                    currentBranches.Insert(branchIndex, newBranch);
                            }
                            else
                                break;
                        }
                    }
                    if (!foundInstantly)
                    {
                        int nextIndex = inputLines.IndexOf(diffLines[diffIndexStart].Original, inputIndex);
                        if (nextIndex != -1)
                        {
                            DiffBranch addBranch = new DiffBranch
                            {
                                parent = currentBranch,
                                diffIndexStart = diffIndexStart,
                                diffIndexEnd = diffIndexStart,
                                inputIndexStart = inputIndex,
                                inputIndexEnd = nextIndex,
                                cumulativeDeletions = currentBranch.cumulativeDeletions,
                                cumulativeAdds = currentBranch.cumulativeAdds + (nextIndex - inputIndex),
                                cumulativeFounds = currentBranch.cumulativeFounds + 1,
                                diffIndexNext = diffIndexStart + 1,
                                inputIndexNext = nextIndex + 1,
                            };
                            if (addBranch.BetterThan(bestBranch))
                                if (addBranch.inputIndexNext == inputLines.Count)
                                    bestBranch = addBranch;
                                else
                                    currentBranches.Insert(branchIndex, addBranch);
                        }
                        else if (inputIndex != inputLines.Count - 1)
                        {
                            DiffBranch addBranch = new DiffBranch
                            {
                                parent = currentBranch,
                                diffIndexStart = diffIndexStart,
                                diffIndexEnd = diffIndexStart,
                                inputIndexStart = inputIndex,
                                inputIndexEnd = inputIndex + 1,
                                cumulativeDeletions = currentBranch.cumulativeDeletions,
                                cumulativeAdds = currentBranch.cumulativeAdds + 1,
                                cumulativeFounds = currentBranch.cumulativeFounds,
                                diffIndexNext = diffIndexStart,
                                inputIndexNext = inputIndex + 1,
                            };
                            if (addBranch.BetterThan(bestBranch))
                                if (addBranch.inputIndexNext == inputLines.Count)
                                    bestBranch = addBranch;
                                else
                                    currentBranches.Insert(branchIndex, addBranch);
                        }
                    }
                }
            }
            if (Cmds.LogCmd.debugChatLogs)
                Console.WriteLine($"\nTested {count} DiffBranches");
            return bestBranch;
        }

        public static void ExecuteDiff(List<DiffLine> diffBase, IEnumerable<string> inputBase, DiffOptions options)
        {
            DiffBranch bestBranch = GetBestDiffBranch(diffBase, inputBase);
            bestBranch.execute(diffBase, new List<string>(inputBase), options);
        }

        [Flags]
        public enum DiffOptions
        {
            None = 0,
            AddFirst = 1,
        }

        public static List<string> DiffLinesToList(List<DiffLine> lines)
        {
            List<string> newList = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                var diffLine = lines[i];
                foreach (string line in diffLine.New)
                    newList.Add(line);
                if (!diffLine.Remove)
                    newList.Add(diffLine.Original);
            }
            return newList;
        }

        public static string DiffLinesToString(List<DiffLine> lines)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string value in DiffLinesToList(lines))
            {
                stringBuilder.AppendLine(value);
            }
            int length = Environment.NewLine.Length;
            if (stringBuilder.Length > length)
            {
                stringBuilder.Length -= length;
            }
            return stringBuilder.ToString();
        }

        public static List<string> DiffLinesToListInfo(List<DiffLine> lines)
        {
            List<string> newList = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                var diffLine = lines[i];
                foreach (string line in diffLine.New)
                    newList.Add("+ " + line);
                if (!diffLine.Remove)
                    newList.Add("| " + diffLine.Original);
                else
                    newList.Add("- " + diffLine.Original);
            }
            return newList;
        }

        public static string DiffLinesToStringInfo(List<DiffLine> lines)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string value in DiffLinesToListInfo(lines))
            {
                stringBuilder.AppendLine(value);
            }
            int length = Environment.NewLine.Length;
            if (stringBuilder.Length > length)
            {
                stringBuilder.Length -= length;
            }
            return stringBuilder.ToString();
        }
    }
}
