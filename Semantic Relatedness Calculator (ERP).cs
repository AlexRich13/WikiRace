using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

public class Program
{
    public static void Search(List<string> discovered, List<string> queue, List<string> origin) //function for the search of links within each wikipedia page
    {
        string link = queue[0]; //initialises the Wikipedia page at the front of the queue to be searched
        WebClient client = new WebClient(); //initialises the use of Wikimedia API
        queue.RemoveAt(0); //removes the current page from the front of the queue so that the next iteration uses the next page
        string text = client.DownloadString("https://en.wikipedia.org/wiki/" + link); //downloads the pages HTML source
        List<char> templinkstring = new List<char>(); //initialises the list in which every character of the discovered link will be stored
        int ptstart = text.IndexOf("<p>") + 3; //initialises the start of the text to be searched at ' <p>, which is HTML's indicator for the start of plain text
        int ptend = text.IndexOf("id=\"See_also") - 30; // initialises the end of the text to break the search at the start of the see also page
        if (ptend == -31) //if there is no match, text.IndexOf() is set to -1, therefore -31 will be the value if there is no See also page after the -30 operation
        {
            ptend = text.IndexOf("id=\"References") - 30; //if there is no see also page, set the end of the page to the start of the reference page
            if (ptend == -31)
            {
                ptend = text.Length - 30; //if there is no reference page, set the end of the page to the last unique character across the HTML for all Wikipedia pages
            }
        }
        List<char> textlist = new List<char>(); //(lines 23 - 28) removes text outside of established start and end and replaces the old text with the new, shortened text
        List<int> linkindex = new List<int>();
        textlist.AddRange(text);
        textlist.RemoveRange(ptend, textlist.Count - ptend);
        textlist.RemoveRange(0, ptstart);
        text = string.Concat(textlist);
        ptstart = 0; //initialises the starting point for the search at the start of the text character list
        while (true) //forever statement
        {
            templinkstring.Clear(); //removes link stored in string from previous iteration
            int i = ptstart; //iterative variable set to start of page text
            while (true)
            {
                if (i >= text.Length - 13) //if the pointer is on a character within the plain text;
                {
                    return;
                }
                int j; // iterative variable for collection of letters within the current link
                if (text[i] == '<' && text[i + 1] == 'a' && text[i + 2] == ' ' && text[i + 3] == 'h' && text[i + 9] == '/' && text[i + 10] == 'w' && text[i + 11] == 'i' && text[i + 12] == 'k' && text[i + 13] == 'i')
                { //if the character the pointer is on, and the subsequent 13 letters, is equal to "<a href="wiki" (the HTML indicator for a wiki link")
                    j = i + 15; //start the collection of letters within the link after the link indicator
                    while (j < i + 300) //while the name of the link is within the length of the longest link on wikipedia
                    {
                        if ((text[j] == ':') || (text[j] == '(' && text[j + 1] == 'i' && text[j + 2] == 'd' && text[j + 3] == 'e' && text[j + 4] == 'n' && text[j + 5] == 't' && text[j + 6] == 'i' && text[j + 7] == 'f' && text[j + 8] == 'i' && text[j + 9] == 'e' && text[j + 10] == 'r' && text[j + 11] == ')') || (text[j] == 'L' && text[j + 1] == 'i' && text[j + 2] == 's' && text[j + 3] == 't' && text[j + 4] == '_'))
                        { //omit any link if it starts with :(identifier), which are links to irregular wiki pages
                            goto bin;
                        }
                        if (text[j] != '"') //if the end of the link name has not been reached yet
                        {
                            templinkstring.Add(text[j]); //add the character the pointer is currently on to the name of the current link being collected
                            j++; //change pointer to the next letter to be added to the name of the current link
                            continue;
                        }
                        goto join;
                    }
                }
                i++;
                continue;
            join:
                string linkin = string.Concat(templinkstring); //concatenates the list of letters for the current link into a string
                if (!discovered.Contains(linkin)) //(lines 63 - 68) if the link that has been found has not already been discovered, add it to the links discovered, queue of links to be searched, and the page the link was found on
                {
                    discovered.Add(linkin);
                    queue.Add(linkin);
                    origin.Add(link);
                }
                ptstart = i + 15 + linkin.Length; //set the start point of the search algorithm to after the link which was just found 
                break;
            bin:
                ptstart = j + 12; //set the start point of the search algorithm to after the ommited link which was just found 
                break;
            }
        }
    }

    public static void Main()
    {
        Console.WriteLine("Semantic Relatedness Calculator\n\nEnter topic (must have a page on wikipedia, capitalise first word and proper nouns, must have _ instead of space:)");
        string link = Console.ReadLine();
        string startpage = link; //different variables must be used as link is dynamic whereas startpage is static and must be used for building of final path
        Console.WriteLine("\nEnter second topic (must have a page on wikipedia, capitalise first word and proper nouns, must have _ instead of space:)");
        string root = Console.ReadLine();
        string endpage = root; //different variables must be used as root is dynamic whereas endpage is static and must be used for building of final path
        Console.WriteLine("\nComputing shortest path...");
        List<string> discovered = new List<string>(); // (lines 85 - 92) initialises BFS algorithm
        List<string> origin = new List<string>(); //not part of traditional BFS algorithm, however needed in this algorithm to trace back each link to the link it came from
        List<string> queue = new List<string>();
        List<string> path = new List<string>();
        path.Add(root);
        discovered.Add(link);
        queue.Add(link);
        origin.Add(null);
        while (!discovered.Contains(root)) //while the end-page has not been discovered
        {
            Search(discovered, queue, origin); //run the search algorithm with all neccessary parameters for BFS algorithm
        }
        while (!path.Contains(startpage)) //(after the end-page has been found) while the path does not contain the start-page, so the arrangement of path has not yet been completed;
        {
            root = origin[discovered.IndexOf(root)]; //set the origin of the link to the origin documented for the link currently at the end of the link path
            path.Insert(0, root); //insert the origin of the previous link to the start of the path of links
        }
        string match;
        if (path.Count == 2)
        {
            match = "closely";
        }
        else if (path.Count == 3)
        {
            match = "broadly";
        }
        else if (path.Count == 4)
        {
            match = "slightly";
        }
        else
        {
            match = "sparsely";
        }
        Console.WriteLine("\n" + startpage + " is " + match + " related to " + endpage + ", with degree of seperation of " + (path.Count - 1)); //output the level of semantic relation
        Console.WriteLine("\n" + String.Join(" > ", path)); //outputs the link path from start-page to end-page
    }
}
