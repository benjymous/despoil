﻿using System.Text.RegularExpressions;
using despoil;
using Fluid;

var inputFilename = "issues.txt";
var fullFilename = Path.Combine(Directory.GetCurrentDirectory(), inputFilename);

Console.WriteLine("Reading input");
string text = File.ReadAllText(inputFilename).Replace("\r", "");

Console.WriteLine("Parsing data");
var entries = text.Split("\n\n");

var issues = new List<string>();
var threads = new Dictionary<string, List<string>>();
var threadKey = new Dictionary<string, string>();
var entityKey = new Dictionary<string, string>();
var entityAlias = new Dictionary<string, string>();
var entityAppearance = new Dictionary<string, HashSet<string>>();
var entityCount = new Dictionary<string, int>();

var collections = new Dictionary<string, List<(string, string)>>();

var colourStyles = new List<string>();

var seenDates = new HashSet<double>();

var dateMarkers = new Dictionary<long, string>();
var dateAppearance = new Dictionary<long, HashSet<string>>();

var issueOrder = new List<string>();

var collectionData = new Dictionary<string, List<IssueGroup>>();
var threadData = new Dictionary<string, List<IssueGroup>>();
var issueData = new List<Issue>();
var entityData = new List<Entity>();
var itemData = new List<Item>();

double currentDate = 0;
const double Increment = 0.0001;

var eventDates = new List<(int, double)>();

List<string> reformatted = new List<string>();

int lineNumber = 0;
foreach (var entry in entries)
{
    lineNumber++;
    int eventCount = 0;

    var entryLines = entry.Split("\n").Select(e => e.Trim()).ToArray();

    if (entryLines.Length == 1)
    {
        lineNumber++;
        continue;
    }

    reformatted.Add(entryLines[0]);
    var reformattedEntryLines = entryLines.Skip(1).Select(e => " " + Util.SortAndDedupeEntities(e)).ToList();
    reformatted.AddRange(reformattedEntryLines);
    if (reformattedEntryLines.Count == 4)
    {
        reformatted.Add(" ?? TODO");
        lineNumber++;
    }
    reformatted.Add("");

    if (entryLines.Length < 4)
    {
        Console.WriteLine($"{fullFilename}({lineNumber}): Error: not enough lines in entry");
        throw new InvalidDataException();
    }

    for (int i = 5; i < entryLines.Length; i++)
    {
        if (!(entryLines[i].StartsWith('#') || entryLines[i].StartsWith("-") || entryLines[i].StartsWith("?") || entryLines[i].StartsWith("!")))
        {
            Console.WriteLine($"{fullFilename}({lineNumber + i}): Error: '{entryLines[i]}' should start with #, -, ?, or !");
            throw new InvalidDataException();
        }
    }

    var entryEvents = entryLines.Skip(5).ToList();
    if (entryEvents.Count == 0)
    {
        lineNumber += entryLines.Length;
        continue;
    }

    var thread = entryLines[1].Trim();
    var issueTitle = $"<span class='itemIssue'>{entryLines[0].Trim()}</span><span class='itemTitle'>{entryLines[2]}</span>";
    var issueTitlePlain = $"{entryLines[0].Trim()} - {entryLines[2]}";
    try
    {
        currentDate = Util.ParseDate(entryLines[3]);
    }
    catch (Exception)
    {
        Console.WriteLine($"{fullFilename}({lineNumber + 3}): Error: '{entryLines[3]}' bad date");
        throw new InvalidDataException();
    }

    if (!threads.ContainsKey(thread))
    {
        threadKey[thread] = $"t{threads.Count}";
        threads[thread] = new List<string>();
    }

    var entryThreadKey = threadKey[thread];
    var issueId = $"{entryThreadKey}_{threads[thread].Count}";

    if (thread != "--")
    {
        issueOrder.Add(issueId);
    }

    threads[thread].Add(issueTitle);
    issues.Add(issueTitlePlain);

    var issueCollections = entryLines[4].Split(",").Select(x => x.Trim());
    foreach (var issueCollection in issueCollections)
    {
        if (!collections.ContainsKey(issueCollection))
        {
            collections[issueCollection] = new List<(string, string)>();
        }

        collections[issueCollection].Add((issueTitle, issueId));
    }

    var dateStr = "&nbsp;";
    bool knownDate = false;
    foreach (var e in entryEvents)
    {
        HashSet<string> entryEntities = new HashSet<string>();

        if (e.StartsWith("?"))
        {
            if (e.StartsWith("??") && (!(e == "?? TODO")))
            {
                Console.WriteLine($"{fullFilename}({lineNumber + 5 + entryEvents.IndexOf(e)}): Info: '{e}'");
            }
            continue;

        }

        var ev = e;

        MatchCollection matches = Regex.Matches(e, "<.*?>");

        List<string> entityClasses = new List<string>();

        foreach (Match match in matches)
        {
            var tag = match.ToString();
            var bareText = match.ToString().Replace("<", "").Replace(">", "");
            var entity = "";
            var alias = "";

            if (bareText.Contains('|'))
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
                entity = $"e_{entityKey.Count}";
                entityKey[alias] = entity;
            }
            else
            {
                entity = entityKey[alias];
            }

            if (!entityAppearance.ContainsKey(entity))
            {
                entityAppearance[entity] = new HashSet<string>();
                entityCount[entity] = 0;
            }
            entityCount[entity]++;

            entityClasses.Add(entity);

            if (entryEntities.Contains(bareText))
            {
                Console.WriteLine($"{fullFilename}({lineNumber + eventCount + 5}): Warning: Duplicate entity <{bareText}>");
            }
            else
            {
                entryEntities.Add(bareText);
            }

            entityAppearance[entity].Add(issueId);

            var title = alias == bareText ? "" : $"title='{alias.Replace("'", "&apos;")}'";
            var resultTag = $"<span {title} class='entity {entity}' onclick='togglePush(\"{entity}\")'>{bareText}</span>";

            ev = ev.Replace(tag, resultTag);
        }

        var evBody = "";
        var slimEvent = ev.StartsWith("-");

        if (ev.StartsWith("## ") || ev.StartsWith("-# "))
        {
            var bits = ev.Split(':');
            dateStr = bits[0].Substring(2);

            if (dateStr.Contains('*'))
            {
                dateStr = dateStr.Replace("*", "");
                knownDate = true;
            }
            else
            {
                knownDate = false;
            }
            try
            {
                currentDate = Util.ParseDate(dateStr);
            }
            catch (System.Exception)
            {
                Console.WriteLine($"{fullFilename}({lineNumber + eventCount + 5}): Error: Bad date: `{dateStr}`");
                throw;
            }
            if (dateStr.Contains('|'))
            {
                dateStr = dateStr.Substring(dateStr.IndexOf("|") + 1);
            }
            else if (dateStr.Contains('!'))
            {
                dateStr = "";
            }
            else
            {
                dateStr = dateStr.Replace("~", "approx ");

                if (dateStr.Contains('.'))
                {
                    dateStr = dateStr.Substring(0, dateStr.IndexOf("."));
                }

                if (dateStr.Contains('-'))
                {
                    dateStr = dateStr.Replace("-", "");
                    dateStr += " BC";
                }
                if (currentDate >= 0 && currentDate < 1000)
                {
                    dateStr += " AD";
                }
            }
            dateStr = dateStr.Replace("  ", " ");

            evBody = string.Join(":", bits.Skip(1));
        }
        else if (ev.StartsWith("#- ") || ev.StartsWith("-- "))
        {
            evBody = ev.Substring(2);

            currentDate += Increment;
        }
        else
        {
            evBody = ev;
        }

        if (thread == "--")
        {
            currentDate = double.MinValue;
        }

        if (evBody.Contains("++"))
        {
            var additional = evBody.Substring(evBody.IndexOf("++") + 2).Trim();

            evBody = evBody.Substring(0, evBody.IndexOf("++"));
            evBody += $"<span class='extras'><details><summary>..involving..</summary>{additional}</details></span>";
        }

        evBody = evBody.Replace("[", "<").Replace("]", ">");

        while (seenDates.Contains(currentDate))
        {
            currentDate += Increment;
        }

        long centuryKey = long.MinValue;
        long decadeKey = long.MinValue;

        if (currentDate <= -4500000000)
        {
            centuryKey = long.MinValue + 100;
            if (!dateMarkers.ContainsKey(centuryKey))
            {
                dateMarkers.Add(centuryKey, $"distant past");
            }
        }
        else if (currentDate > -4500000000 && currentDate <= -1000000)
        {
            centuryKey = -4500000000;
            if (!dateMarkers.ContainsKey(centuryKey))
            {
                dateMarkers.Add(centuryKey, $"earth's past");
            }
        }
        else if (currentDate > -1000000 && currentDate <= -0)
        {
            centuryKey = -1000000;
            if (!dateMarkers.ContainsKey(centuryKey))
            {
                dateMarkers.Add(centuryKey, $"human history");
            }
        }
        else if (currentDate > 0 && currentDate < 10000)
        {
            int century = 1 + ((int)currentDate / 100);
            centuryKey = (century - 1) * 100;

            if (!dateMarkers.ContainsKey(centuryKey))
            {
                dateMarkers.Add(centuryKey, $"{Util.NumberOrdinal(century)} century");
            }

            if (currentDate >= 1900 && currentDate < 3000)
            {
                decadeKey = (int)currentDate / 10 * 10;

                if (!dateMarkers.ContainsKey(decadeKey))
                {
                    dateMarkers.Add(decadeKey, $"{decadeKey}s");
                }
            }
        }
        else if (currentDate > 10000)
        {
            centuryKey = 10000;
            if (!dateMarkers.ContainsKey(centuryKey))
            {
                dateMarkers.Add(centuryKey, $"far future");
            }
        }

        if (centuryKey != long.MinValue)
        {
            if (!dateAppearance.ContainsKey(centuryKey))
            {
                dateAppearance[centuryKey] = new HashSet<string>();
            }
            dateAppearance[centuryKey].Add(issueId);
        }

        if (decadeKey != long.MinValue)
        {
            if (!dateAppearance.ContainsKey(decadeKey))
            {
                dateAppearance[decadeKey] = new HashSet<string>();
            }
            dateAppearance[decadeKey].Add(issueId);
        }


        if (eventCount == 0)
        {
            if (thread != "--")
            {
                // Add dummy entry for issue
                seenDates.Add(currentDate);
                eventDates.Add((eventDates.Count + 1, currentDate));

                itemData.Add(new Item
                {
                    type = "Issue",
                    id = issueId,
                    threadkey = entryThreadKey,
                    subtitle = $"{entryLines[3]} - {issueTitle}",
                });
            }

        }

        eventCount++;

        seenDates.Add(currentDate);
        eventDates.Add((eventDates.Count + 1, currentDate));

        itemData.Add(new Item
        {
            type = thread != "--" ? "Item" : "Intro",
            slim = slimEvent,
            id = thread != "--" ? issueId : Util.MakeId(evBody.Trim()),
            body = evBody.Trim(),
            subtitle = $"{entryLines[3]} - {issueTitle}",
            issueLaneGroup = entryLines[1],
            threadkey = entryThreadKey,
            dateval = currentDate,
            date = dateStr.Trim(),
            dateclass = knownDate ? "itemDate knownDate" : "itemDate",
            entities = string.Join(" ", entityClasses),
        });
    }

    lineNumber += entryLines.Length;
}

File.WriteAllLines(inputFilename, reformatted);

foreach (var marker in dateMarkers)
{
    eventDates.Add((eventDates.Count + 1, marker.Key - 0.00001));
    itemData.Add(new Item
    {
        type = "Date",
        entities = string.Join(" ", dateAppearance[marker.Key].Select(x => "ei_" + x)),
        body = marker.Value
    });
}

int threadIdx = 0;

var colNames = collections.Keys.OrderBy(x => x, new GroupComparer()).ToArray();

foreach (var collection in colNames)
{
    if (collection == "--") continue;

    string groupName = collection.Contains(':') ? collection.Split(':')[0] : collection;

    if (!collectionData.ContainsKey(groupName))
    {
        collectionData[groupName] = new List<IssueGroup>();
    }

    var colData = collections[collection];
    var colIndex = Array.IndexOf(colNames, collection);

    collectionData[groupName].Add(new IssueGroup
    {
        id = $"col_{colIndex}",
        name = collection.Contains(':') ? collection.Substring(collection.IndexOf(":") + 2) : collection,
        index = colIndex,
        childdata = string.Join(",", colData.Select(x => x.Item2)),
        Issues = colData.Select(x => new Issue { id = x.Item2, body = x.Item1 }).ToArray()
    });
}

foreach (var thread in threads)
{
    if (thread.Key == "--") continue;

    string groupName = thread.Key.Contains(':') ? thread.Key.Split(':')[0] : thread.Key;

    if (!threadData.ContainsKey(groupName))
    {
        threadData[groupName] = new List<IssueGroup>();
    }

    var key = threadKey[thread.Key];

    var childKeys = string.Join(",", Enumerable.Range(0, thread.Value.Count).Select(i => $"{key}_{i}"));

    var color = Util.Rainbow(threads.Count + 5, threadIdx++);

    colourStyles.Add($".{key} {{ border-color: {color}; }} ");
    colourStyles.Add($".{key}_f {{ border-color: {color}; background: {color}22; }} ");

    int idx = 0;
    threadData[groupName].Add(new IssueGroup
    {
        id = key,
        name = thread.Key.Contains(':') ? thread.Key.Split(":")[1] : thread.Key,
        index = 0,
        childdata = childKeys,
        Issues = thread.Value.Select(x => new Issue { id = $"{key}_{idx++}", body = x }).ToArray()
    });
}

foreach (var thread in threads)
{
    if (thread.Key == "--") continue;
    int idx = 0;
    issueData.AddRange(thread.Value.Select(issue => new Issue { id = $"{threadKey[thread.Key]}_{idx++}", body = issue, hash = Util.MakeId(issue).Substring(0, 8) }));
}

issueData = issueOrder.Join(issueData, i => i, d => d.id, (i, d) => d).ToList();

entityData.AddRange(entityKey.OrderBy(x => Util.MoveThe(x.Key)).Select(entity => new Entity
{
    id = entity.Value,
    name = Util.MoveThe(entity.Key),
    count = entityCount[entity.Value],
    issues = string.Join(" ", entityAppearance[entity.Value].Select(x => "ei_" + x))
}));

var sortedDates = eventDates.Select(x => x.Item2).OrderBy(i => i).ToArray();

var orderStyles = eventDates.Select(x => $".chron :nth-child({x.Item1}) {{ order: {Array.IndexOf(sortedDates, x.Item2) + 1} }}");

//////////////////////////////////////

var reverseAlias = entityAlias.Where(kvp => kvp.Key[0].IsUppercase()).GroupBy(x => Util.MoveThe(x.Value), x => Util.MoveThe(x.Key));
var entityDict = entityData.ToDictionary(e => e.name, e => e);

try
{
    var entityListIn = File.ReadAllLines("issues.entities.txt").Select(line => line.Trim());
    string currentGroup = "-";
    foreach (var line in entityListIn)
    {
        if (string.IsNullOrWhiteSpace(line)) continue;
        if (line.StartsWith("::"))
        {
            currentGroup = line.Substring(2);
        }
        else
        {
            var name = "";
            var notes = "";
            if (line.Contains(" - "))
            {
                var bits = line.Split(" - ");
                name = bits[0].Trim();
                notes = bits[1].Trim();
            }
            else
            {
                name = line.Trim();
            }
            if (entityDict.TryGetValue(name, out var e))
            {
                e.type = currentGroup;
                e.notes = notes;
            }
        }
    }
}
catch { }

var entitiesByType = entityData.GroupBy(e => e.type);

List<string> entityListOut = new();

foreach (var group in entitiesByType.OrderBy(g => g.Key))
{
    entityListOut.Add("::" + group.Key);
    foreach (var e in group)
    {
        entityListOut.Add(" " + e.name + (string.IsNullOrWhiteSpace(e.notes) ? "" : " - " + e.notes));
    }
    entityListOut.Add("");
}
File.WriteAllLines("issues.entities.txt", entityListOut);

foreach (var reverse in reverseAlias)
{
    if (!string.IsNullOrWhiteSpace(entityDict[reverse.Key].notes)) entityDict[reverse.Key].notes += "\n";
    entityDict[reverse.Key].notes += "( " + string.Join(", ", reverse) + " )";
}

//////////////////////////////////////

Dictionary<string, string> laneDict = new();
List<string> allLaneGroups = new();

try
{
    var laneListIn = File.ReadAllLines("issues.lanes.txt").Select(line => line.Trim());
    string currentGroup = "";
    foreach (var line in laneListIn)
    {
        if (string.IsNullOrWhiteSpace(line)) continue;
        if (line.StartsWith("::"))
        {
            currentGroup = line.Substring(2).Trim();
        }
        else
        {
            var title = line.Trim();
            laneDict[title] = currentGroup;
            if (!allLaneGroups.Contains(currentGroup)) allLaneGroups.Add(currentGroup);
        }
    }
}
catch { }



foreach (var item in itemData)
{
    item.issueLane = laneDict.ContainsKey(item.issueLaneGroup) ? laneDict[item.issueLaneGroup] : "--";
    item.issueLaneId = "Lane" + allLaneGroups.IndexOf(item.issueLane).ToString();
}

var groups = itemData.Select(i => i.issueLane).Distinct();

List<string> issueListOut = new();

foreach (var group in groups)
{
    issueListOut.Add("::" + group);

    foreach (var v in itemData.Where(i => i.issueLane == group).Select(i => i.issueLaneGroup).Where(v => !string.IsNullOrWhiteSpace(v) && v != "--").Distinct())
    {
        issueListOut.Add(" " + v);
    }

    issueListOut.Add("");

}

File.WriteAllLines("issues.lanes.txt", issueListOut);

//////////////////////////////////////

var parser = new FluidParser();

var source = File.ReadAllText("template.html");

var model = new Model
{
    InlineStyles = $"<style>{string.Join(" ", colourStyles)} {string.Join(" ", orderStyles)}</style>",
    Collections = collectionData.Select(x => new IssueGroupParent { name = x.Key, Groups = x.Value.ToArray() }).ToArray(),
    Threads = threadData.Select(x => new IssueGroupParent { name = x.Key, Groups = x.Value.ToArray() }).OrderBy(x => x.name, new GroupComparer()).ToArray(),
    Issues = issueData.ToArray(),
    EntityGroups = entitiesByType.Select(x => new EntityGroup { name = x.Key, Entities = x.ToArray(), issues = string.Join(" ", x.Select(e => e.issues)) }).OrderBy(g => g.name).ToArray(),
    Items = itemData.ToArray(),
    AllIssues = string.Join(" ", issueOrder.Select(x => "ei_" + x))
};

var options = new TemplateOptions();
options.MemberAccessStrategy.Register<Model>();
options.MemberAccessStrategy.Register<IssueGroupParent>();
options.MemberAccessStrategy.Register<IssueGroup>();
options.MemberAccessStrategy.Register<Issue>();
options.MemberAccessStrategy.Register<EntityGroup>();
options.MemberAccessStrategy.Register<Entity>();
options.MemberAccessStrategy.Register<Item>();

if (parser.TryParse(source, out var template, out var error))
{
    var context = new TemplateContext(model, options);

    var outPath = Path.Combine(Directory.GetCurrentDirectory(), "out");
    Console.WriteLine($"Rendering html output");
    var rendered = Util.RemoveEmptyLines(template.Render(context));

    Console.WriteLine($"Writing output");
    var outFile = Path.Combine(outPath, "index.html");
    File.WriteAllText(outFile, rendered, System.Text.Encoding.UTF8);
    Console.WriteLine($"Written to {outFile}");
}
else
{
    Console.WriteLine($"Error: {error}");
}