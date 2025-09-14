using System.Windows.Input;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace DCCPanelController.View.Components;

/// <summary>
/// A lightweight, fully-native Markdown renderer for .NET MAUI (no WebView).
/// Supports: headings, paragraphs, bold/italic, inline code, fenced code blocks,
/// links (clickable spans), quotes, unordered/ordered lists, horizontal rules.
/// </summary>
public class MarkdownLabel : ContentView {

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(MarkdownLabel),
            propertyChanged: (b, _, __) => ((MarkdownLabel)b).Render());

    /// <summary>The Markdown text to render.</summary>
    public string? Text {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(MarkdownLabel),
            14.0, // default
            propertyChanged: (b, _, __) => ((MarkdownLabel)b).Render());

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty FontColorProperty =
        BindableProperty.Create(
            nameof(FontColor),
            typeof(Color),
            typeof(MarkdownLabel),
            Colors.Black, // default
            propertyChanged: (b, _, __) => ((MarkdownLabel)b).Render());

    public Color FontColor
    {
        get => (Color)GetValue(FontColorProperty);
        set => SetValue(FontColorProperty, value);
    }

    
    public static readonly BindableProperty LinkTappedCommandProperty =
        BindableProperty.Create(
            nameof(LinkTappedCommand),
            typeof(ICommand),
            typeof(MarkdownLabel));

    /// <summary>
    /// Optional command invoked when a link span is tapped.
    /// Parameter: string URL.
    /// If not set, links will be opened with Launcher.OpenAsync.
    /// </summary>
    public ICommand? LinkTappedCommand {
        get => (ICommand?)GetValue(LinkTappedCommandProperty);
        set => SetValue(LinkTappedCommandProperty, value);
    }

    // ===== Internals =====

    readonly VerticalStackLayout _stack;
    readonly MarkdownPipeline    _pipeline;

    public MarkdownLabel() {
        // Layout container for generated controls
        _stack = new VerticalStackLayout {
            Spacing = 6,
            Padding = new Thickness(0),
        };

        // You can wrap in a ScrollView externally if you want scrolling
        Content = _stack;

        // Configure Markdig
        _pipeline = new MarkdownPipelineBuilder()
                   .UseAdvancedExtensions() // tables, lists, etc. (we’ll render a subset)
                   .Build();
    }

    void Render() {
        _stack.Children.Clear();
        var md = Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(md)) return;

        var doc = Markdown.Parse(md, _pipeline);
        foreach (var block in doc) {
            switch (block) {
                case HeadingBlock heading:
                    _stack.Children.Add(RenderHeading(heading));
                break;

                case ParagraphBlock paragraph:
                    _stack.Children.Add(RenderParagraph(paragraph));
                break;

                case ListBlock list:
                    RenderList(list, _stack, 0);
                break;

                case QuoteBlock quote:
                    _stack.Children.Add(RenderQuote(quote));
                break;

                case ThematicBreakBlock:
                    _stack.Children.Add(RenderRule());
                break;

                case FencedCodeBlock fenced:
                    _stack.Children.Add(RenderCodeBlock(fenced));
                break;

                case CodeBlock code:
                    _stack.Children.Add(RenderCodeBlock(code));
                break;

                default:
                    // Fallback: render as plain paragraph text
                    if (block is LeafBlock { Inline: { } } lb)
                        _stack.Children.Add(RenderParagraph(new ParagraphBlock() { Inline = lb.Inline }));
                break;
            }
        }
    }

    // ===== Block renderers =====

    Microsoft.Maui.Controls.View RenderHeading(HeadingBlock h) {
        var label = new Label {
            LineBreakMode = LineBreakMode.WordWrap,
            FontAttributes = FontAttributes.Bold,
            TextColor = FontColor,

            // Sizes roughly inspired by HTML h1..h6
            FontSize = h.Level switch {
                1 => 28,
                2 => 24,
                3 => 20,
                4 => 18,
                5 => 16,
                _ => 14
            },
            FormattedText = ToFormattedString(h.Inline),
        };

        return label;
    }

    Microsoft.Maui.Controls.View RenderParagraph(ParagraphBlock p) {
        var lbl = new Label {
            LineBreakMode = LineBreakMode.WordWrap,
            FontSize = FontSize,
            TextColor = FontColor,
            FormattedText = ToFormattedString(p.Inline),
        };
        return lbl;
    }

    Microsoft.Maui.Controls.View RenderCodeBlock(LeafBlock codeBlock) {
        // Get raw text (CodeBlock/ FencedCodeBlock content)
        var lines = codeBlock.Lines.Lines;
        var text = string.Join(Environment.NewLine, lines.Select(l => l.Slice.ToString()));

        var lbl = new Label {
            LineBreakMode = LineBreakMode.WordWrap,
            FontFamily = "Courier New, Menlo, Consolas, monospace",
            FontSize = FontSize,
            TextColor = FontColor,
            Text = text.TrimEnd('\r', '\n')
        };

        // Stylish frame around code
        var frame = new Border {
            Padding = new Thickness(10),
            Margin = new Thickness(0, 6, 0, 6),
            BackgroundColor = new Color(0.97f, 0.97f, 0.97f),
            Background = new Color(0.88f, 0.88f, 0.88f),
            Content = lbl
        };

        return frame;
    }

    Microsoft.Maui.Controls.View RenderQuote(QuoteBlock q) {
        var container = new Grid {
            ColumnDefinitions = {
                new ColumnDefinition { Width = 6 },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };

        // Vertical bar
        container.Add(new BoxView {
            Color = new Color(0.75f, 0.75f, 0.75f),
            Margin = new Thickness(0, 2, 8, 2)
        }, 0, 0);

        var contentStack = new VerticalStackLayout { Spacing = 4 };
        foreach (var block in q) {
            switch (block) {
                case ParagraphBlock p:
                    contentStack.Children.Add(RenderParagraph(p));
                break;

                case ListBlock list:
                    RenderList(list, contentStack, 0);
                break;

                case HeadingBlock h:
                    contentStack.Children.Add(RenderHeading(h));
                break;

                case FencedCodeBlock fenced:
                    contentStack.Children.Add(RenderCodeBlock(fenced));
                break;

                case CodeBlock code:
                    contentStack.Children.Add(RenderCodeBlock(code));
                break;

                default:
                break;
            }
        }
        container.Add(contentStack, 1, 0);
        return container;
    }

    Microsoft.Maui.Controls.View RenderRule() => new BoxView {
        HeightRequest = 1,
        BackgroundColor = new Color(0.85f, 0.85f, 0.85f),
        Margin = new Thickness(0, 8, 0, 8)
    };

    void RenderList(ListBlock list, Layout layout, int indentLevel) {
        int index = 1;
        foreach (var itemObj in list) {
            if (itemObj is not ListItemBlock item) continue;

            // For each item, gather paragraph-like content into one label line if possible
            var itemStack = new VerticalStackLayout { Spacing = 4, Margin = new Thickness(indentLevel * 16, 0, 0, 0) };

            // Bullet / number prefix
            string prefix = list.IsOrdered ? $"{index}. " : "• ";

            foreach (var sub in item) {
                switch (sub) {
                    case ParagraphBlock p: {
                        var lbl = new Label {
                            LineBreakMode = LineBreakMode.WordWrap,
                            FontSize = FontSize,
                            TextColor = FontColor
                        };

                        var fs = ToFormattedString(p.Inline);

                        // Prepend bullet/number to first paragraph only
                        if (!string.IsNullOrWhiteSpace(prefix)) {
                            fs.Spans.Insert(0, new Span { Text = prefix });
                            prefix = string.Empty; // consume
                        }

                        lbl.FormattedText = fs;
                        itemStack.Children.Add(lbl);
                        break;
                    }

                    case ListBlock nested:
                        RenderList(nested, itemStack, indentLevel + 1);
                    break;

                    case FencedCodeBlock fenced:
                        // If bullet prefix still not consumed, put it before the code label
                        if (!string.IsNullOrWhiteSpace(prefix)) {
                            itemStack.Children.Add(new Label { Text = prefix, FontSize = FontSize, TextColor = FontColor });
                            prefix = string.Empty;
                        }
                        itemStack.Children.Add(RenderCodeBlock(fenced));
                    break;

                    default:
                        // Fallback
                    break;
                }
            }

            // If we never consumed the prefix (e.g., empty item), add an empty paragraph
            if (!string.IsNullOrWhiteSpace(prefix)) {
                itemStack.Children.Add(new Label { Text = prefix, FontSize = FontSize, TextColor = FontColor });
            }

            layout.Children.Add(itemStack);
            index++;
        }
    }

    // ===== Inline renderer (Paragraph/Heading text) =====

    FormattedString ToFormattedString(ContainerInline? inlineRoot) {
        var fs = new FormattedString();
        if (inlineRoot == null) return fs;

        void EmitLiteral(string? text, FontAttributes attr = FontAttributes.None) {
            if (string.IsNullOrEmpty(text)) return;
            fs.Spans.Add(new Span {
                Text = text,
                FontAttributes = attr
            });
        }

        void EmitCode(string? text) {
            if (string.IsNullOrEmpty(text)) return;
            fs.Spans.Add(new Span {
                Text = text,
                FontFamily = "Courier New, Menlo, Consolas, monospace",
                BackgroundColor = new Color(0.95f, 0.95f, 0.95f)
            });
        }

        void EmitLink(string text, string url) {
            var span = new Span {
                Text = text,
                TextDecorations = TextDecorations.Underline,
                TextColor = Colors.Blue
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, __) => {
                if (LinkTappedCommand?.CanExecute(url) == true)
                    LinkTappedCommand.Execute(url);
                else
                    try {
                        await Launcher.Default.OpenAsync(new Uri(url));
                    } catch { /* ignore */
                    }
            };
            span.GestureRecognizers.Add(tap);
            fs.Spans.Add(span);
        }

        void WalkInline(Inline? node, FontAttributes currentAttr) {
            if (node == null) return;

            switch (node) {
                case LiteralInline lit:
                    EmitLiteral(lit.Content.ToString(), currentAttr);
                break;

                case EmphasisInline emph:
                    var added = currentAttr;

                    //if (emph.IsDouble) added |= FontAttributes.Bold;
                    if (emph.DelimiterCount == 2) added |= FontAttributes.Bold;
                    else added |= FontAttributes.Italic;

                    foreach (var c in emph)
                        WalkInline(c, added);
                break;

                case LineBreakInline:
                    fs.Spans.Add(new Span { Text = Environment.NewLine });
                break;

                case CodeInline code:
                    EmitCode(code.Content);
                break;

                case LinkInline link when link.Url != null: {
                    // Render link text (children) as plain string to keep it simple
                    string text = string.Concat(link.Select(child => {
                        if (child is LiteralInline lit2) return lit2.Content.ToString();
                        if (child is CodeInline ci) return ci.Content;
                        return string.Empty;
                    }));
                    EmitLink(string.IsNullOrWhiteSpace(text) ? link.Url : text, link.Url);
                    break;
                }

                default:
                    // Walk children if any
                    if (node is ContainerInline container)
                        foreach (var c in container)
                            WalkInline(c, currentAttr);
                break;
            }
        }

        foreach (var child in inlineRoot)
            WalkInline(child, FontAttributes.None);

        return fs;
    }
}