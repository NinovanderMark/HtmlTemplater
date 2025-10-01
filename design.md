Html Templater v1
- Define custom HTML elements in separate component file
- InnerHtml should be replaced with all HTML contained in the parent element
- Proper HTML elements can be redefined to be custom elements
- Referring to a custom element from its definition results in that literal HTML element being used instead of the custom one

-- index.htmt
```
<html>
	<body>
		<section>
			<p>Paragraph goes here</p>
		</section>
	</body>
</htmL>
```

-- section.htmt
```
<section class="row">
	{{ InnerHtml }}
</section>
```

- Set up a simple example website created with Html Templater

-- index.htmt
```
<page>
	<p>This is how you use HTML Templater</p>
</page>
```

-- page.htmt
```
<html>
	<head>
		<link rel="stylesheet" href="/assets/styles.css" type="text/css">
	</head>
	<body>
		<header></header>
		<main>
			{{ InnerHtml }}
		</main>
	</body>
</html>
```

- Simple folder structure separating assets, elements and pages
- Everything in the assets folder will be copied to an assets folder in the output root
- Each page should be recreated in the output folder, resolving all elements
- Manifest file should index the elements defined, and can be used for configuration

- src
	- assets
		styles.css
	- elements
		page.htmt
		section.htmt
	- pages
		- programming
			blogpost.htmt
			photograph.jpg
		index.htmt
	manifest.json