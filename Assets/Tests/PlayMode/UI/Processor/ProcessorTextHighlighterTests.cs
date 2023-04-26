using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ButterBoard;
using ButterBoard.UI.Processor;
using NUnit.Framework;
using Toaster.Execution;
using UnityEngine;

namespace Tests.PlayMode.UI.Processor
{
    public class ProcessorTextHighlighterTests
    {
        private string InstructionColor => GetColorTag(TextHighlightSettings.Instance.instructionColor);
        private string RegisterColor => GetColorTag(TextHighlightSettings.Instance.registerColor);
        private string LabelColor => GetColorTag(TextHighlightSettings.Instance.labelColor);
        private string LabelArgColor => GetColorTagWithItalics(TextHighlightSettings.Instance.labelColor);
        private string BeginError => GetErrorTag(TextHighlightSettings.Instance.errorColor);
        private string ConstantColor => GetColorTag(TextHighlightSettings.Instance.constantColor);
        private string PinColor => GetColorTag(TextHighlightSettings.Instance.pinColor);
        private string CommentColor => GetColorTagWithItalics(TextHighlightSettings.Instance.commentColour);
        private string Newline => Environment.NewLine;
        private string EndColor => "</color>";
        private string EndColorItalics => "</color></i>";
        private string EndError => "</u>";

        private string GetColorTag(Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">";
        }

        private string GetErrorTag(Color color)
        {
            return "<u color=#" + ColorUtility.ToHtmlStringRGB(color) + ">";
        }

        private string GetColorTagWithItalics(Color color)
        {
            return "<i>" + GetColorTag(color);
        }

        private string GetHighlightedText(string input)
        {
            ExecutionConfig executionConfig = new ExecutionConfig()
            {
                NamedRegisters = new List<string>(),
                BasicRegisterCount = 4,
                MaxStackDepth = 20,
                PinCount = 20,
                StackRegisterCount = 4,
            };

            ProcessorTextHighlighter highlighter = new ProcessorTextHighlighter(input, executionConfig);
            highlighter.Parse();
            return highlighter.CompileText();
        }

        [Test]
        public void SingleLineColours()
        {
            string expected = $"{InstructionColor}mov </color>{RegisterColor}$r1 </color>{RegisterColor}$r0</color>";
            Assert.AreEqual(expected, GetHighlightedText("mov $r1 $r0"));
        }

        [Test]
        public void SingleLineColours_Error_Instruction()
        {
            string expected = $"{InstructionColor}{BeginError}moo {EndError}</color>{RegisterColor}$r1 </color>{RegisterColor}$r0</color>";
            Assert.AreEqual(expected, GetHighlightedText("moo $r1 $r0"));
        }

        [Test]
        public void SingleLineColours_Error_Arg()
        {
            string expected = $"{InstructionColor}mov </color>{RegisterColor}{BeginError}$r10 {EndError}</color>{RegisterColor}$r0</color>";
            Assert.AreEqual(expected, GetHighlightedText("mov $r10 $r0"));
        }

        [Test]
        public void SingleLineColours_Error_BothArgs()
        {
            string expected = $"{InstructionColor}mov </color>{RegisterColor}{BeginError}$r10 </u></color>{RegisterColor}{BeginError}$r</u></color>";
            Assert.AreEqual(expected, GetHighlightedText("mov $r10 $r"));
        }

        [Test]
        public void SingleLineColours_Error_All()
        {
            string expected = $"{InstructionColor}{BeginError}moo {EndError}</color>{RegisterColor}{BeginError}$r10 {EndError}</color>{RegisterColor}{BeginError}$r{EndError}</color>";
            Assert.AreEqual(expected, GetHighlightedText("moo $r10 $r"));
        }

        [Test]
        public void MultiLineColours()
        {
            string expected = $"{LabelColor}:label{EndColor}{Newline}{InstructionColor}jmp {EndColor}{LabelArgColor}label{EndColorItalics}";
            Assert.AreEqual(expected, GetHighlightedText($":label{Newline}jmp label"));
        }

        [Test]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void ProgramInSteps()
        {
            string[] expected = new []{
                $"{InstructionColor}{BeginError}m{EndError}{EndColor}",
                $"{InstructionColor}{BeginError}mo{EndError}{EndColor}",
                $"{InstructionColor}{BeginError}mov{EndError}{EndColor}",
                $"{InstructionColor}{BeginError}mov{EndError}{EndColor} ",
                $"{InstructionColor}{BeginError}mov {EndError}{EndColor}{BeginError}${EndError}",
                $"{InstructionColor}{BeginError}mov {EndColor}{RegisterColor}{BeginError}$r{EndError}{EndColor}{EndError}",
                $"{InstructionColor}{BeginError}mov {EndColor}{RegisterColor}$r0{EndColor}{EndError}",
                $"{InstructionColor}{BeginError}mov {EndColor}{RegisterColor}$r0{EndColor}{EndError} ",
                $"{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{ConstantColor}3{EndColor}",
                $"{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{ConstantColor}30{EndColor}",
                $"{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{ConstantColor}30{EndColor}{CommentColor};{EndColorItalics}",
                $"{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{ConstantColor}30{EndColor}{CommentColor}; {EndColorItalics}",
                $"{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{ConstantColor}30{EndColor}{CommentColor}; c{EndColorItalics}",
                $"{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{ConstantColor}30{EndColor}{CommentColor}; co{EndColorItalics}",
                $"{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{ConstantColor}30{EndColor}{CommentColor}; com{EndColorItalics}",
                $"{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{ConstantColor}30{EndColor}{CommentColor}; comm{EndColorItalics}",
                $"{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{ConstantColor}30{EndColor}{CommentColor}; comme{EndColorItalics}",
                $"{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{ConstantColor}30{EndColor}{CommentColor}; commen{EndColorItalics}",
                $"{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{ConstantColor}30{EndColor}{CommentColor}; comment{EndColorItalics}",
            };

            string program = "mov $r0 30; comment";

            for (int i = 0; i < program.Length; i++)
            {
                string checkingLine = program.Substring(0, i + 1);

                string expectedLine;
                try
                {
                    expectedLine = expected[i];
                }
                catch (IndexOutOfRangeException)
                {
                    Assert.Fail($"No expected result for index {i}");
                    return;
                }

                // individual watch
                if (i == 10)
                    Debugger.Break();

                Assert.AreEqual(expectedLine, GetHighlightedText(checkingLine), $"failed on character index {i}");
            }
        }

        [Test]
        public void Issue_InvalidTokenVanishesPrecedingText()
        {
            /*
             * _ word
             * mov $r0 $r1
             */
            string expected = $"{BeginError}_ word{EndError}{Newline}{InstructionColor}mov {EndColor}{RegisterColor}$r0 {EndColor}{RegisterColor}$r1{EndColor}";
            Assert.AreEqual(expected, GetHighlightedText($"_ word{Newline}mov $r0 $r1"));
        }
    }
}