﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Html.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class AutoLinkFixture : BaseFixture
    {
        public class ExecuteTests : AutoLinkFixture
        {
            [Test]
            public void NoReplacementReturnsSameDocument()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                        </body>
                    </html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobaz", "http://www.google.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results, Is.EquivalentTo(new[] { document }));
            }

            [Test]
            public void AddsLink()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <a href=""http://www.google.com"">Foobar</a> text</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void AddsLinkWithAlternateQuerySelector()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <baz>This is some Foobar text</baz>
                            <p>This is some Foobar text</p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <baz>This is some <a href=""http://www.google.com"">Foobar</a> text</baz>
                            <p>This is some Foobar text</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                }).WithQuerySelector("baz");

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void AddsLinkWhenContainerHasChildElements()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some Foobar <b>text</b></p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some <a href=""http://www.google.com"">Foobar</a> <b>text</b></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void AddsLinkWhenInsideChildElement()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some <i>Foobar</i> <b>text</b></p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some <i><a href=""http://www.google.com"">Foobar</a></i> <b>text</b></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void DoesNotReplaceInAttributes()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1 title=""Foobar"">Title</h1>
                            <p attr=""Foobar"">This is some Foobar <b ref=""Foobar"">text</b></p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1 title=""Foobar"">Title</h1>
                            <p attr=""Foobar"">This is some <a href=""http://www.google.com"">Foobar</a> <b ref=""Foobar"">text</b></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void AddsMultipleLinksInSameElement()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foobar</a></i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void AddsMultipleLinksInDifferentElements()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foobaz</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                            <p>Another Foobaz paragraph</p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foobaz</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foobar</a></i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                            <p>Another <a href=""http://www.bing.com"">Foobaz</a> paragraph</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void DoesNotAddLinksInExistingLinkElements()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <a href=""http://www.yahoo.com"">Foobar</a> text Foobaz</p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <a href=""http://www.yahoo.com"">Foobar</a> text <a href=""http://www.bing.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void AddsMultipleLinksWhenFirstIsSubstring()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foo</a>bar</i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void AddLinkMethodTakesPrecedence()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foobar</a></i> text <a href=""http://www.yahoo.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                }).WithLink("Foobaz", "http://www.yahoo.com");

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void IgnoreSubstringIfSearchingForWholeWords()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foo</i> text Foobaz</p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foo</a></i> text Foobaz</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                }).WithMatchOnlyWholeWord();

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void AdjacentWords()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>abc Foo(baz) xyz</p>
                        </body>
                    </html>";
                string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>abc <a href=""http://www.google.com"">Foo</a>(<a href=""http://www.yahoo.com"">baz</a>) xyz</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                    { "baz", "http://www.yahoo.com" },
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output.Replace("\r\n", "\n") }));
            }

            [Test]
            public void NonWholeWords()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>abc Foo(baz) xyz</p>
                        </body>
                    </html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                    { "baz", "http://www.yahoo.com" },
                }).WithMatchOnlyWholeWord();

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results, Is.EquivalentTo(new[] { document }));
            }
        }
    }
}