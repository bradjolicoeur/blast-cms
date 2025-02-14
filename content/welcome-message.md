# Welcome!

Thank you for choosing Blast CMS for your headless content management system.  

Below you will find some helpful tips to get you started with your website development.

## Getting Started

1. Create your website layout using the tool of your choice or select a previously created layout for Blast CMS as a starting point
2. Create content items that you will integrate into your site.  This can be placeholder content for now.
3. Start integrating the Blast CMS API into your website layout.  You can have the content owner update the placeholder text while you do this.
4. Make sure all of your content is final.
5. Launch your website!

## Tips

- Once you have placeholder content in place it makes it easy to hand off content editing to the content owners to work on in parallel.
- Deploying the website front end with a random undiscoverable URL will make previewing changes with the content editor or stakeholders much easier.
- Caching content in your front end will make your site significantly faster and ensure you do not run into throttling limits.

## Editing Content

Editing content in Blast CMS is very easy.  If you can fill out a simple form, you can edit content in Blast CMS.  This works well for small to medium sized business and individuals who need to edit content but have other competing responsibilities.  There is no technical coding skills or design skills required.

To edit content, 

1. Go to the content type that you need to edit on the left navigation bar and select it.  
2. You will find a grid of existing content.  Select one for edit or click the add button at the bottom  of the grid to add a new one.
3. Edit your content in the form and save it
4. Done!

> Content can even be edited on your smart phone or tablet, so you can quickly edit content while on the go.

### Markdown Text
Many of the fields in Blast CMS are intended to support Markdown text.  Blast CMS does not convert the Markdown to HTML, so you will need to do this in your display.  This gives you the most flexibility and keeps Blast CMS simple.  There are many Markdown to HTML libraries in all languages that make the conversion extremely easy to implement.

Markdown is a very simple way to format text for websites that gives the editor lots of control over the layout with a minimum of technical knowledge and is the recommended formatting method for content.  One of the many benefits of Markdown is that you can mix actual HTML in with Markdown syntax.  This means you will not end up in edge cases where markdown will not work.  

[**Markdown Cheat Sheet**](https://www.markdownguide.org/cheat-sheet/)

## Integrating with Content API

The [API documentation](swagger/index.html) is linked at the bottom of the left nav. You will find a page that documents the endpoints as well as the OpenApi.json file that will allow you to generate an SDK in the language of your choice.  At this time there is no published SDK, but you will find that the API is designed to be very simple and accessible without an SDK.

Authentication to the API is done through an API Key passed as the `ApiKey` header in each request.  You can generate a key by navigating to [Settings/Api Keys](apikeys).  Here you can create new API Keys and Expire keys that you are no longer using. 

> The only time you will be able to see the API key is when you create it.  If you loose the key, you will need to generate a new one.

Use a Read only Api Key for embedding into your website.  Only use Full Access Key for scenarios where you are migrating content in bulk or creating content based on an integration with another system.  

## Content Types

There is a standard set of content types in Blast CMS that support a typical website.  These content types can be used in a variety of ways, but they all have an intended purpose.  Below is a description of what the content types are designed to be used for, feel free to get creative in your usage.

### Images

Images is a library of images that are used in your website.  When you upload images they are automatically compressed for fast page loads.  This saves you time in your workflow and removes the need for content owners to understand how to compress images.  

Images show up in drop downs in some of the content types like Landing Pages and Blog Articles.  

In addition, you can copy the Image URL to reference an image directly in your content.

### Landing Pages

Landing pages can be used for any page in your website or for marketing landing pages.  

### Content Groups and Content Blocks

Content Blocks are used for any block of content and can be used independently or you can create a group and join the Content Block to one or more groups of content.

The Content Groups make it efficient to fetch a set of Content Blocks for rendering in your page.  For example, often home pages have multiple sections.  You may have a 'Home Page' Content Group that has multiple Content Blocks for your home page.  Joining the Content Blocks to the 'Home Page' content group allows you to make one API call for all the Content Blocks on the home page.

### Blog Articles

Blog Articles are specifically for a blog article or any type of news article or story you want to create.

### Event Venues

Event Venues are for location details like the address, lat/lon and the website.

You can link Event Venues from Events or use Event Venues independent of events for something like store locations for your contact page.

### Events

Events are for providing information about an event like a comedy show, music performance, farmers market, etc.  

### Podcasts

Podcast contains the listing information for a Podcast.  This is used in conjunction with Podcast Episodes to create an RSS feed or landing page for a Podcast.

### Podcast Episodes

Podcast Episodes are for listing each episode of a Podcast.  You can use this for the description of the episode, link to the media file and a cover image.

### Feed Articles

Feed Articles is a special feature I added for my personal blog site and you can use it as well.  

Feed Articles are links to articles, YouTube episodes or other interesting websites that I run across and add to the list.  I then publish that list as a news feed on my site.

This content type has an AI feature that will scan a URL you provide, summarize the content and add it to the list.  

### URL Redirects

URL Redirects are used for storing redirections when you have pages that have been removed or renamed.  

Redirecting pages that were removed or renamed is important to ensure your users to not end up with dead links. 

### Sitemap Items

Sitemap items are for storing pages to build a sitemap.  This makes generating a sitemap dynamically a relatively simple task.

### Content Tags

Content Tags are used within other content types like Blog Articles as a way to implement features like filtering articles by tag.

### Email Templates

Email Templates can be used for storing transactional email templates. 