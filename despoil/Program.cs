using System.Text.RegularExpressions;
using despoil;
using Fluid;
using System.Linq;

string text = File.ReadAllText("issues.txt").Replace("\r", "");
var entries = text.Split("\n\n\n");

var issues = new List<string>();
var threads = new Dictionary<string, List<string>>();
var threadKey = new Dictionary<string, string>();
var entityKey = new Dictionary<string, string>();
var entityAlias = new Dictionary<string, string>();
var entityAppearence = new Dictionary<string, HashSet<string>>();

var collections = new Dictionary<string, List<(string, string)>>();

var colourStyles = new List<string>();

var seenDates = new HashSet<double>();

var dateMarkers = new Dictionary<long, string>();
var dateAppearence = new Dictionary<long, HashSet<string>>();

var allIssues = new HashSet<string>();

var collectionData = new List<IssueGroup>();
var threadData = new List<IssueGroup>();
var issueData = new List<Issue>();
var entityData = new List<Entity>();
var itemData = new List<Item>();

double currentDate = 0;
const double Increment = 0.0001;

var eventDates = new List<(int, double)>();

foreach (var entry in entries)
{
  int eventCount = 0;

  var entrylines = entry.Split("\n");
  if (entrylines.Length < 4)
  {
    throw new Exception("Error with " + entrylines[0]);
  }

  for (int i = 5; i < entrylines.Length; i++)
  {

    if (!(entrylines[i].StartsWith('#') || entrylines[i].StartsWith("?")))
    {
      throw new Exception($"Error with {entrylines[0]}\n{entrylines[i]}' should start with #");
    }
  }

  var thread = entrylines[1].Trim();
  var issueTitle = $"<span class='itemissue'>{entrylines[0].Trim()}</span><span class='itemtitle'>{entrylines[2]}</span>";
  var issueTitlePlain = $"{entrylines[0].Trim()} - {entrylines[2]}";
  currentDate = Util.ParseDate(entrylines[3]);

  if (!threads.ContainsKey(thread))
  {
    threadKey[thread] = $"t{threads.Count}";
    threads[thread] = new List<string>();
  }

  var threadkey = threadKey[thread];
  var issueId = $"{threadkey}_{threads[thread].Count}";

  allIssues.Add(issueId);

  threads[thread].Add(issueTitle);
  issues.Add(issueTitlePlain);

  var issueCollections = entrylines[4].Split(",").Select(x => x.Trim());
  foreach (var issueCollection in issueCollections)
  {
    if (!collections.ContainsKey(issueCollection))
    {
      collections[issueCollection] = new List<(string, string)>();
    }

    collections[issueCollection].Add((issueTitle, issueId));
  }

  var entryEvents = entrylines.Skip(5).ToList();
  if (entryEvents.Count == 0)
  {
    entryEvents.Add(issueTitlePlain);
  }

  var dateStr = "&nbsp;";
  foreach (var e in entryEvents)
  {
    if (e.StartsWith("?")) continue;

    var ev = e;

    MatchCollection matches = Regex.Matches(e, "<.*?>");

    List<string> entityClasses = new List<string>();

    foreach (Match match in matches)
    {
      var tag = match.ToString();
      var bareText = match.ToString().Replace("<", "").Replace(">", "");
      var entity = "";
      var alias = "";

      if (bareText.Contains("|"))
      {
        var bits = bareText.Split("|");
        bareText = bits[0];
        alias = bits[1];
        if (!entityAlias.ContainsKey(bareText))
        {
          entityAlias[bareText] = alias;
        }
      }
      else
      {
        if (entityAlias.ContainsKey(bareText))
        {
          alias = entityAlias[bareText];
        }
        else
        {
          alias = bareText;
        }
      }

      if (!entityKey.ContainsKey(alias))
      {
        entity = $"entity_{entityKey.Count}";
        entityKey[alias] = entity;
      }
      else
      {
        entity = entityKey[alias];
      }

      if (!entityAppearence.ContainsKey(entity))
      {
        entityAppearence[entity] = new HashSet<string>();
      }

      entityClasses.Add(entity);
      entityAppearence[entity].Add(issueId);

      var title = alias == bareText ? "" : $"title='{alias}'";
      var resultTag = $"<span {title} class='entity {entity}' onclick='togglePush(\"{entity}\")'>{bareText}</span>";

      ev = ev.Replace(tag, resultTag);
    }


    var evBody = "";


    if (ev.StartsWith("## "))
    {
      var bits = ev.Split(':');
      dateStr = bits[0].Substring(2);
      currentDate = Util.ParseDate(dateStr);

      if (dateStr.Contains("|"))
      {
        dateStr = dateStr.Substring(dateStr.IndexOf("|") + 1);
      }
      else
      {
        dateStr = dateStr.Replace("~", "approx");

        if (dateStr.Contains("."))
        {
          dateStr = dateStr.Substring(0, (dateStr.IndexOf(".")));
        }


        if (dateStr.Contains("-"))
        {
          dateStr = dateStr.Replace("-", "");
          dateStr += " BC";
        }
        if (currentDate >= 0 && currentDate < 1000)
        {
          dateStr += " AD";
        }
      }

      evBody = String.Join(":", bits.Skip(1));
    }
    else if (ev.StartsWith("# "))
    {
      evBody = ev.Substring(2);

      currentDate += Increment;
    }
    else
    {
      evBody = ev;
    }

    if (evBody.Contains("++"))
    {
      var additional =  evBody.Substring(evBody.IndexOf("++")+2).Trim();
      
      evBody = evBody.Substring(0, evBody.IndexOf("++"));
      evBody += $"<span class='extras'><details><summary>..involving..</summary>{additional}</details></span>";
    }

    while (seenDates.Contains(currentDate))
    {
      currentDate += Increment;
    }

    int centuryKey = int.MinValue;
    int decadeKey = int.MinValue; ;

    if (currentDate > 0 && currentDate < 3000)
    {
      int century = 1 + ((int)currentDate / 100);
      centuryKey = (century - 1) * 100;

      if (!dateMarkers.ContainsKey(centuryKey))
      {
        dateMarkers.Add(centuryKey, $"{Util.NumberOrdinal(century)} century");
      }

      if (currentDate >= 1900 && currentDate < 3000)
      {
        decadeKey = ((int)currentDate / 10) * 10;

        if (!dateMarkers.ContainsKey(decadeKey))
        {
          dateMarkers.Add(decadeKey, $"{decadeKey}s");
        }
      }
    }

    if (centuryKey != int.MinValue)
    {
      if (!dateAppearence.ContainsKey(centuryKey))
      {
        dateAppearence[centuryKey] = new HashSet<string>();
      }
      dateAppearence[centuryKey].Add(issueId);
    }

    if (decadeKey != int.MinValue)
    {
      if (!dateAppearence.ContainsKey(decadeKey))
      {
        dateAppearence[decadeKey] = new HashSet<string>();
      }
      dateAppearence[decadeKey].Add(issueId);
    }


    if (eventCount == 0)
    {
      // Add dummy entry for issue
      seenDates.Add(currentDate);
      eventDates.Add((eventDates.Count + 1, currentDate));

      itemData.Add(new Item
      {
        type = "Issue",
        id = issueId,
        threadkey = threadkey,
        subtitle = $"{entrylines[3]} - {issueTitle}"
      });

    }

    eventCount++;

    seenDates.Add(currentDate);
    eventDates.Add((eventDates.Count + 1, currentDate));

    itemData.Add(new Item
    {
      type = "Item",
      id = issueId,
      body = evBody,
      subtitle = $"{entrylines[3]} - {issueTitle}",
      threadkey = threadkey,
      dateval = currentDate,
      date = dateStr,
      entities = string.Join(" ", entityClasses),
    });
  }
}


foreach (var marker in dateMarkers)
{
  eventDates.Add((eventDates.Count + 1, marker.Key - 0.00001));
  itemData.Add(new Item
  {
    type = "Date",
    entities = String.Join(" ", dateAppearence[marker.Key].Select(x => "entityissue_" + x)),
    body = marker.Value
  });
}

int threadIdx = 0;

var colnames = collections.Keys.OrderBy(x => x, new CollectionComparer()).ToArray();

foreach (var collection in colnames)
{
  var coldata = collections[collection];
  var colidx = Array.IndexOf(colnames, collection);

  collectionData.Add(new IssueGroup
  {
    id = $"col_{colidx}",
    name = collection,
    index = colidx,
    childdata = string.Join(",", coldata.Select(x => x.Item2)),
    Issues = coldata.Select(x => new Issue { id = x.Item2, body = x.Item1 }).ToArray()
  });
}

foreach (var thread in threads)
{
  var key = threadKey[thread.Key];

  var childkeys = string.Join(",", Enumerable.Range(0, thread.Value.Count).Select(i => $"{key}_{i}"));

  colourStyles.Add($".{key}");
  colourStyles.Add("{");
  colourStyles.Add($"  border: 2px solid {Util.Rainbow(threads.Count + 5, threadIdx++)};");
  colourStyles.Add("}");

  int idx = 0;
  threadData.Add(new IssueGroup
  {
    id = key,
    name = thread.Key,
    index = 0,
    childdata = childkeys,
    Issues = thread.Value.Select(x => new Issue { id = $"{key}_{idx++}", body = x }).ToArray()
  });
}

foreach (var thread in threads)
{
  int idx = 0;
  issueData.AddRange(thread.Value.Select(issue => new Issue { id = $"{threadKey[thread.Key]}_{idx++}", body = issue }));
}

entityData.AddRange(entityKey.OrderBy(x => Util.MoveThe(x.Key)).Select(entity => new Entity
{
  id = entity.Value,
  name = Util.MoveThe(entity.Key),
  issues = string.Join(" ", entityAppearence[entity.Value].Select(x => "entityissue_" + x))
}));

var outpath = Path.Combine(Directory.GetCurrentDirectory(), "out");

Console.WriteLine($"Writing to {outpath}");

File.WriteAllLines(Path.Combine(outpath, "colors.css"), colourStyles);

var sortedDates = eventDates.Select(x => x.Item2).OrderBy(i => i).ToArray();

File.WriteAllLines(Path.Combine(outpath, "order0.css"), eventDates.Select(x => $".box :nth-child({x.Item1}) {{ order: {x.Item1} }}"));
File.WriteAllLines(Path.Combine(outpath, "order1.css"), eventDates.Select(x => $".box :nth-child({x.Item1}) {{ order: {Array.IndexOf(sortedDates, x.Item2) + 1} }}"));

//////////////////////////////////////

var parser = new FluidParser();

var source = File.ReadAllText("template.html");

var model = new Model
{
  Collections = collectionData.ToArray(),
  Threads = threadData.ToArray(),
  Issues = issueData.ToArray(),
  Entities = entityData.ToArray(),
  Items = itemData.ToArray(),
  AllIssues = string.Join(" ", allIssues.Select(x => "entityissue_" + x))
};

var options = new TemplateOptions();
options.MemberAccessStrategy.Register<Model>();
options.MemberAccessStrategy.Register<IssueGroup>();
options.MemberAccessStrategy.Register<Issue>();
options.MemberAccessStrategy.Register<Entity>();
options.MemberAccessStrategy.Register<Item>();

if (parser.TryParse(source, out var template, out var error))
{
  var context = new TemplateContext(model, options);

  File.WriteAllText(Path.Combine(outpath, "index.html"), Util.RemoveEmptyLines(template.Render(context)));
}
else
{
  Console.WriteLine($"Error: {error}");
}