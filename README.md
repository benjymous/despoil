# _**De**spoil_ - The (hopefully) less spoilery Sandman Universe timeline 

Hello!  Welcome to _**De**spoil_ - The (hopefully) less spoilery Sandman Universe timeline.

This github repo contains multiple moving parts, the list of comic issues (issues.txt) which is parsed by the _despoil_ C# application, and rendered using a server side template into static html.  When the page is displayed, there are a few javascript helpers that handle turning stuff on and off.

## Contributing

* `issues.txt` contains the raw data for all the issues - this is processed by the despoil C# application into html and css files, which get auto deployed by netlify.  The simplest way of contributing is to create a Pull Request and make some edits to this file.   Note it has fairly specific syntax, which will need to work for the parser to correctly generate the html, so don't go removing linespace or the various header lines.
* The `despoil` folder contains the C# application.  This could probably use a bit of tidying.   I discovered C# 6 lets you write console applications as a big flat file without the normal class/namespace hierarchy, and by the time I realised I was probably making a mistake, I'd already written a lot of code!   The iteration now is better than it was, as it uses `template.html` to build the resultant page using [fluid](https://github.com/sebastienros/fluid) as the actual templating engine, which is based on the [liquid](https://shopify.github.io/liquid/) syntax.   There are still places the code builds html directly, so that could do with tidying up!
* The `out` folder is where the generated files go, and is also the home to the static css and javascript files - this keeps things simple when editing and tweaking the css and javascript.

## FAQ

* **Why is it called despoil?** It's a bad pun, really.  Something to prevent spoilage.  Also it sounds a bit like it could be a member of the Endless if you weren't listening properly...

* **Why did you put an event on a particular date?**  Bold dates are ones that are explicitly mentioned in the text - those are Anchor dates which are then used to derive other dates.   Also seasons are mentioned in a few places (autumn winds, midsummer sun, etc, can help pin events to particular times of the year.)  Likewise some characters mention their age in a few places (or in the case of children, are born, and grow up during the story) so these again give approximate timespans.   Other times it's just guesswork, and when all else fails, I just assume the stories are set roughly around when the issues would've been written or published.   Historical events, or events based on existing mythology have been placed roughly in the right time frames, but again, I'm no historian, so there are bound to be mistakes. **If you have evidence a date is wrong, please let me know!**

* **Why don't you mention _specific event_ ?**  One of the aims of this site is not to contain massive spoilers.   That's why I've tried to write the event summaries in a fairly vague way - the intention is to jog the reader's memory when they think "Hmm, what happened in that issue", or "did those two characters meet?"

* **Why don't you include events from _other comic_?**  I can only write about what I've read, and so far that's the main 75 issue run of The Sandman, plus most of Gaiman's additional Sandman and Sandman-adjacent works, including the two Death mini series, the original Books of Magic mini series, and the re-issued Children's Crusade.  I next plan to work through the other post-Sandman works by other authors, but there has to be a cutoff point somewhere - especially when the newer (post-2018) issues start contradicting dates (e.g. by reintroducing characters who are suddenly a vastly different age.)   **If there's something you really want to include, then you can find my Ko-fi link and Amazon Wishlists on the despoil site!**

* **What do all the different colours mean?**  The colours are auto generated from the story arcs - it's intended to make it easier to see when events from different issues end up meshing together in the chronological view.

* **What about the Audible and Netflix versions?**  This is all part of the plan - obviously there will be some continuity differences between the TV show and the original books - notably that it's set in the 2020s rather than the late 1980s, but once the show is out, I hope to add a way that'll let a viewer see which TV episodes map to which comic issues.  Audible should be simpler, as it's a more direct translation from the books.

## TODO

* Add filtering based on author / artist
* Include some way of browsing by TV episode, or Audible chapter
