window.MathJax = {
  loader: { load: ['[tex]/ams'] },
  tex: {
    packages: { '[+]': ['ams'] },
    inlineMath: [['\\(', '\\)'], ['$', '$']],
    displayMath: [['\\[', '\\]'], ['$$', '$$']],
    processEscapes: true,
    processEnvironments: true
  },
  options: {
    // Only process elements with class 'arithmatex' by default. 
    // Do NOT set ignoreHtmlClass to ".*" here; use a conservative ignore class if needed.
    //ignoreHtmlClass: ".*",
    processHtmlClass: "arithmatex"
  }
};

document$.subscribe(() => {
  // If MathJax hasn't loaded yet, just skip this navigation event
  if (!window.MathJax || !MathJax.typesetPromise) return;

  MathJax.startup.output.clearCache();
  MathJax.typesetClear();
  MathJax.texReset();
  MathJax.typesetPromise();
});


// Helper: wrap simple inline TeX in nav labels with a span that MathJax will process
function wrapNavTex() {
  var nav = document.querySelector('.md-nav');
  if (!nav) return;

  // Candidate nodes: links and titles
  var nodes = nav.querySelectorAll('a, .md-nav__title, .md-nav__link');
  nodes.forEach(function (el) {
    if (el.dataset.mathProcessed) return;
    var text = el.textContent;
    if (!text) return;

    // Simple inline math patterns: \(..\) and $..$
    var replaced = text
      .replace(/\\\((.+?)\\\)/g, '<span class="arithmatex">\\($1\\)</span>')
      .replace(/\$(.+?)\$/g, '<span class="arithmatex">\\($1\\)</span>');

    if (replaced !== text) {
      el.innerHTML = replaced;
      el.dataset.mathProcessed = '1';
    }
  });

  return nav;
}

// Typeset only the nav (fast) after wrapping
function typesetNav() {
  if (!window.MathJax || !MathJax.typesetPromise) return;
  var nav = wrapNavTex();
  if (!nav) return;
  MathJax.typesetPromise([nav]).catch(function (err) {
    console.warn('MathJax nav typeset error:', err);
  });
}

// Run on initial load (small delay to let Material build the nav)
window.addEventListener('load', function () {
  setTimeout(typesetNav, 50);
});

// Re-run after Material navigation events
if (window.document$ && typeof window.document$.subscribe === 'function') {
  document$.subscribe(function () {
    setTimeout(typesetNav, 20);
  });
} else {
  window.addEventListener('popstate', function () { setTimeout(typesetNav, 20); });
  window.addEventListener('hashchange', function () { setTimeout(typesetNav, 20); });
}

