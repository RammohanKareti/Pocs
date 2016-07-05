using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WebApiSampleLikeStripe.Helpers
{
    public static class KeyGenerator
    {
        private static Random SpanRandomObject = new Random();
        private static int Key_Payload_Length = 24;
        private static string PASSWORD_CHARS_LCASE = "abcdefgijkmnopqrstwxyz";
        private static string PASSWORD_CHARS_UCASE = "ABCDEFGHJKLMNPQRSTWXYZ";
        private static string PASSWORD_CHARS_NUMERIC = "23456789";


        public static string GenerateSecretKey(string userId, bool isTest)
        {
            var newKey = GenerateRandomKey();
            AddNewSecretKey(newKey, userId, isTest);
            return FormatTHeKey("sk_", isTest, newKey);
        }

        public static string GeneratePublishableKey(string userId, bool isTest)
        {
            var newKey = GenerateRandomKey();
            var secretKey = new PublishableKey
            {
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                IsTest = isTest,
                Value = newKey
            };
            using (var db = new DBEntities())
            {
                db.PublishableKeys.Add(secretKey);
                db.SaveChanges();
            }
            return FormatTHeKey("pk_", isTest, newKey);
        }

        private static string FormatTHeKey(string type, bool isTest, string randomKey)
        {
            var builder = new StringBuilder();
            builder.Append(type);
            builder.Append(isTest ? "test_" : "live_");
            builder.Append(randomKey);
            return builder.ToString();
        }

        private static string GenerateRandomKey()
        {
            char[][] charGroups = new char[][]
            {
                PASSWORD_CHARS_LCASE.ToCharArray(),
                PASSWORD_CHARS_UCASE.ToCharArray(),
                PASSWORD_CHARS_NUMERIC.ToCharArray()
            };

            int[] charsLeftInGroup = new int[charGroups.Length];

            for (int i = 0; i < charsLeftInGroup.Length; i++)
            {
                charsLeftInGroup[i] = charGroups[i].Length;
            }

            int[] leftGroupsOrder = new int[charGroups.Length];

            for (int i = 0; i < leftGroupsOrder.Length; i++)
            {
                leftGroupsOrder[i] = i;
            }

            byte[] randomBytes = new byte[4];

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);

            int seed = BitConverter.ToInt32(randomBytes, 0);

            Random random = new Random(seed);

            Char[] key = new char[Key_Payload_Length];

            int nextCharIndex;

            int nextGroupIndex;

            int nextLeftGroupsOrderIdx;

            // Index of the last non-processed character in a group.
            int lastCharIdx;

            int lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;

            for (int i = 0; i < key.Length; i++)
            {
                if (lastLeftGroupsOrderIdx == 0)
                {
                    nextLeftGroupsOrderIdx = 0;
                }
                else
                {
                    nextLeftGroupsOrderIdx = random.Next(0, lastLeftGroupsOrderIdx);
                }

                nextGroupIndex = leftGroupsOrder[nextLeftGroupsOrderIdx];

                lastCharIdx = charsLeftInGroup[nextGroupIndex] - 1;

                // If only one unprocessed character is left, pick it; otherwise,
                // get a random character from the unused character list.
                if (lastCharIdx == 0)
                {
                    nextCharIndex = 0;
                }
                else
                {
                    nextCharIndex = random.Next(0, lastCharIdx + 1);
                }

                // Add this character to the password.
                key[i] = charGroups[nextGroupIndex][nextCharIndex];

                // If we processed the last character in this group, start over.
                if (lastCharIdx == 0)
                {
                    charsLeftInGroup[nextGroupIndex] =
                                              charGroups[nextGroupIndex].Length;
                }
                // There are more unprocessed characters left.
                else
                {
                    // Swap processed character with the last unprocessed character
                    // so that we don't pick it until we process all characters in
                    // this group.
                    if (lastCharIdx != nextCharIndex)
                    {
                        char temp = charGroups[nextGroupIndex][lastCharIdx];
                        charGroups[nextGroupIndex][lastCharIdx] =
                                    charGroups[nextGroupIndex][nextCharIndex];
                        charGroups[nextGroupIndex][nextCharIndex] = temp;
                    }
                    // Decrement the number of unprocessed characters in
                    // this group.
                    charsLeftInGroup[nextGroupIndex]--;
                }

                // If we processed the last group, start all over.
                if (lastLeftGroupsOrderIdx == 0)
                {
                    lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                }

                // There are more unprocessed groups left.
                else
                {
                    // Swap processed group with the last unprocessed group
                    // so that we don't pick it until we process all groups.
                    if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                    {
                        int temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                        leftGroupsOrder[lastLeftGroupsOrderIdx] =
                                    leftGroupsOrder[nextLeftGroupsOrderIdx];
                        leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                    }
                    // Decrement the number of unprocessed groups.
                    lastLeftGroupsOrderIdx--;
                }
            }

            // Convert password characters into a string and return the result.
            return new string(key);


        }

        private static void AddNewSecretKey(string key, string userId, bool isTestMode)
        {
            var secretKey = new SecretKey
            {
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                IsTest = isTestMode,
                Value = key
            };
            using (var db = new DBEntities())
            {
                var activeSecret = db.SecretKeys.Where(sk => sk.UserId == userId && !sk.IsRevoked && sk.IsTest == isTestMode).FirstOrDefault();
                if (activeSecret != null)
                {
                    activeSecret.IsRevoked = true;
                    activeSecret.RevokedDate = DateTime.UtcNow;
                }
                db.SecretKeys.Add(secretKey);
                db.SaveChanges();
            }
        }

        private static void AddNewPublishableKey(string key, string userId, bool isTestMode)
        {
            var publishableKey = new PublishableKey
            {
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                IsTest = isTestMode,
                Value = key
            };
            using (var db = new DBEntities())
            {
                var activePublishableKey = db.PublishableKeys.Where(pk => pk.UserId == userId && !pk.IsRevoked && pk.IsTest == isTestMode).FirstOrDefault();
                if (activePublishableKey != null)
                {
                    activePublishableKey.IsRevoked = true;
                    activePublishableKey.RevokedDate = DateTime.UtcNow;
                }
                db.PublishableKeys.Add(publishableKey);
                db.SaveChanges();
            }
        }

    }


}