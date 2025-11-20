using UnityEngine;
using System;
using System.Text.RegularExpressions;
using FuzzySharp;

namespace VoiceCommand
{
    /// <summary>
    /// Parses voice commands and executes corresponding actions on TestController
    /// </summary>
    public class CommandParser : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("SpeechRecognitionController to listen to")]
        [SerializeField] private SpeechRecognitionController speechRecognitionController;

        [Tooltip("TestController to execute commands on")]
        [SerializeField] private TestController testController;

        [Header("Matching Settings")]
        [Tooltip("Minimum similarity score (0-100) to accept a command match")]
        [Range(0, 100)]
        [SerializeField] private int similarityThreshold = 70;

        [Header("Debug Options")]
        [Tooltip("Enable detailed logging of command processing")]
        [SerializeField] private bool enableDebugLogs = true;

        // Command definitions with multiple variations
        private readonly CommandDefinition[] _commands = new[]
        {
            new CommandDefinition("MoveLeft", new[] { "왼쪽으로가", "왼쪽으로", "왼쪽", "좌측으로", "좌측" }),
            new CommandDefinition("MoveRight", new[] { "오른쪽으로가", "오른쪽으로", "오른쪽", "우측으로", "우측" }),
            new CommandDefinition("Dance", new[] { "춤춰", "춤추어", "춤을춰", "춤을추어", "댄스" }),
            new CommandDefinition("Wave", new[] { "인사해", "인사", "인사하세요", "손흔들어", "웨이브" }),
            new CommandDefinition("SitOnChair", new[] { "의자에앉아", "앉아", "앉으세요", "의자에앉으세요", "앉기" })
        };

        // Korean + English + Numbers preprocessor
        private readonly Func<string, string> _preprocessor = (input) =>
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Preserve Hangul (U+AC00-U+D7AF) + English + Numbers
            var pattern = @"[^\uAC00-\uD7AF\u1100-\u11FF a-zA-Z0-9]";
            var processed = Regex.Replace(input, pattern, " ");

            // Normalize case
            processed = processed.ToLower();

            // Replace multiple spaces with single space
            processed = Regex.Replace(processed, @"\s+", " ");

            return processed.Trim();
        };

        private void Awake()
        {
            ValidateReferences();
        }

        private void OnEnable()
        {
            if (speechRecognitionController != null)
            {
                speechRecognitionController.onResponse.AddListener(OnVoiceCommandReceived);
                if (enableDebugLogs)
                    Debug.Log("[CommandParser] Registered to SpeechRecognitionController.onResponse");
            }
        }

        private void OnDisable()
        {
            if (speechRecognitionController != null)
            {
                speechRecognitionController.onResponse.RemoveListener(OnVoiceCommandReceived);
                if (enableDebugLogs)
                    Debug.Log("[CommandParser] Unregistered from SpeechRecognitionController.onResponse");
            }
        }

        private void ValidateReferences()
        {
            if (speechRecognitionController == null)
            {
                Debug.LogError("[CommandParser] SpeechRecognitionController is not assigned!");
            }

            if (testController == null)
            {
                Debug.LogError("[CommandParser] TestController is not assigned!");
            }
        }

        /// <summary>
        /// Called when voice command is received from SpeechRecognitionController
        /// </summary>
        private void OnVoiceCommandReceived(string rawInput)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
            {
                if (enableDebugLogs)
                    Debug.LogWarning("[CommandParser] Received empty voice input");
                return;
            }

            if (testController == null)
            {
                Debug.LogError("[CommandParser] Cannot execute command - TestController is not assigned!");
                return;
            }

            // Log preprocessing
            string processedInput = _preprocessor(rawInput);
            LogCommandProcessing(rawInput, processedInput);

            // Find best matching command
            CommandMatch bestMatch = FindBestCommandMatch(processedInput);

            if (bestMatch != null && bestMatch.Score >= similarityThreshold)
            {
                LogMatchResult(bestMatch, true);
                ExecuteCommand(bestMatch.CommandName);
            }
            else
            {
                LogMatchResult(bestMatch, false);
            }
        }

        /// <summary>
        /// Find the best matching command using FuzzySharp
        /// </summary>
        private CommandMatch FindBestCommandMatch(string processedInput)
        {
            CommandMatch bestMatch = null;
            int highestScore = 0;

            foreach (var commandDef in _commands)
            {
                // Use Process.ExtractOne to find best match among variations
                var result = Process.ExtractOne(processedInput, commandDef.Variations, s => _preprocessor(s));

                if (result != null)
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log($"[CommandParser] Command '{commandDef.MethodName}' best match: '{result.Value}' ({result.Score}%)");
                    }

                    if (result.Score > highestScore)
                    {
                        highestScore = result.Score;
                        bestMatch = new CommandMatch
                        {
                            CommandName = commandDef.MethodName,
                            MatchedVariation = result.Value,
                            Score = result.Score
                        };
                    }
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// Execute the matched command on TestController
        /// </summary>
        private void ExecuteCommand(string commandName)
        {
            try
            {
                switch (commandName)
                {
                    case "MoveLeft":
                        testController.MoveLeft();
                        break;

                    case "MoveRight":
                        testController.MoveRight();
                        break;

                    case "Dance":
                        testController.Dance();
                        break;

                    case "Wave":
                        testController.Wave();
                        break;

                    case "SitOnChair":
                        testController.SitOnChair();
                        break;

                    default:
                        Debug.LogWarning($"[CommandParser] Unknown command: {commandName}");
                        break;
                }

                if (enableDebugLogs)
                    Debug.Log($"[CommandParser] ✓ Successfully executed command: {commandName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CommandParser] Error executing command '{commandName}': {ex.Message}");
            }
        }

        #region Logging Methods

        private void LogCommandProcessing(string rawInput, string processedInput)
        {
            if (!enableDebugLogs)
                return;

            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log($"[CommandParser] Raw Input: \"{rawInput}\"");
            Debug.Log($"[CommandParser] Preprocessed: \"{processedInput}\"");
            Debug.Log("───────────────────────────────────────────────────────");
        }

        private void LogMatchResult(CommandMatch match, bool accepted)
        {
            if (!enableDebugLogs)
                return;

            if (accepted && match != null)
            {
                Debug.Log($"[CommandParser] ✓ MATCH FOUND");
                Debug.Log($"[CommandParser]   Command: {match.CommandName}");
                Debug.Log($"[CommandParser]   Variation: \"{match.MatchedVariation}\"");
                Debug.Log($"[CommandParser]   Score: {match.Score}% (Threshold: {similarityThreshold}%)");
            }
            else
            {
                Debug.LogWarning($"[CommandParser] ✗ NO MATCH");
                if (match != null)
                {
                    Debug.LogWarning($"[CommandParser]   Best attempt: \"{match.MatchedVariation}\" ({match.Score}%)");
                    Debug.LogWarning($"[CommandParser]   Required: {similarityThreshold}%");
                }
                else
                {
                    Debug.LogWarning($"[CommandParser]   No similar commands found");
                }
            }
            Debug.Log("═══════════════════════════════════════════════════════");
        }

        #endregion

        #region Nested Classes

        private class CommandDefinition
        {
            public string MethodName { get; }
            public string[] Variations { get; }

            public CommandDefinition(string methodName, string[] variations)
            {
                MethodName = methodName;
                Variations = variations;
            }
        }

        private class CommandMatch
        {
            public string CommandName { get; set; }
            public string MatchedVariation { get; set; }
            public int Score { get; set; }
        }

        #endregion

        #region Editor Helpers

        /// <summary>
        /// Test method for editor testing (can be called from Inspector events)
        /// </summary>
        public void TestCommand(string input)
        {
            OnVoiceCommandReceived(input);
        }

        #endregion
    }
}
