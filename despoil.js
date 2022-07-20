

window.addEventListener('resize', function(event) {
  updateMarkers();
}, true);

setCustomStyle = function (customStyle) {

  /** Replace custom stylesheet */
  if (document.querySelector('link[id="orderStyle"]')) {
    document.head.removeChild(document.querySelector('link[id="orderStyle"]'));
  }
  lk = document.createElement('link');
  lk.setAttribute('id', 'customStyle');
  lk.setAttribute('rel', 'stylesheet');
  lk.setAttribute('href', `${customStyle}.css`);
  document.head.appendChild(lk);
};

showAll = function () {
  setAllIssues(true)
}

showNone = function () {
  setAllIssues(false)
}

setAllIssues = function (enabled) {
  const boxes = document.getElementsByClassName("check_issue")
  for (const box of boxes) {
    box.checked = enabled
    const name = box.id.replace("check_issue_", "");
    setChecked("check_issue_", name, true);
  }

  updateMarkers();
}

setChecked = function (prefix, name, noScroll) {
  const checkBox = document.getElementById(prefix + name);
  const boxes = document.getElementsByClassName(name + "_outer");

  if (!noScroll && checkBox.checked) {
    setTimeout(function () { boxes[0].scrollIntoView({ behavior: "smooth" }) }, 10);
  }

  const styleid = 'style_' + name
  for (const box of boxes) {
    if (checkBox.checked) {
      box.classList.add('displayblock')
      setTimeout(function () { box.classList.add('expanded') }, 0);
      setTimeout(function () { box.classList.add('opacity1') }, 0);


      if (document.getElementById('style_' + name) == null) {
        var style = document.createElement('style')
        style.id = styleid
        style.innerHTML = '.entityissue_' + name + ' { display: block !important; }';
        document.getElementsByTagName('head')[0].appendChild(style);
      }

    } else {
      box.classList.remove('opacity1')
      setTimeout(function () { box.classList.remove('displayblock') }, 100);

      var style = document.getElementById('style_' + name);
      if (style != null) style.remove()
    }
  }

  var cousins = document.getElementsByClassName("check_" + name)
  for (const cousin of cousins) {
    if (cousin != checkBox) {
      cousin.checked = checkBox.checked;
    }
  }

  setParents(name)
}

toggleIssue = function (name) {
  const box = document.getElementById("check_issue_" + name)
  box.checked = !box.checked;
  setChecked("check_issue_", name)
  updateMarkers();
}

togglePush = function (name) {
  const checkBox = document.getElementById("check_" + name);
  checkBox.checked = !checkBox.checked
  setPushed(name)
}

setPushed = function (name) {
  const checkBox = document.getElementById("check_" + name);
  const boxes = document.getElementsByClassName(name);
  for (const box of boxes) {
    if (box.nodeName == 'DIV') {
      if (checkBox.checked) {
        box.classList.add("event_highlight")
        box.parentElement.classList.add("itemrow_highlight")
      } else {
        box.classList.remove("event_highlight")
        box.parentElement.classList.remove("itemrow_highlight")
      }

    } else if (box.nodeName == 'SPAN') {
      if (checkBox.checked) {
        box.classList.add("entity_active")
      } else {
        //box.style = null;
        box.classList.remove("entity_active")
      }
    }
  }
  updateMarkers();
}

highlightNone = function () {
  const boxes = document.getElementsByClassName("check_entity")
  for (const box of boxes) {
    box.checked = false
    const name = box.id.replace("check_", "");
    setPushed(name);
  }
}

setChildren = function (name) {
  const checkBox = document.getElementById("check_" + name);
  const items = checkBox.dataset.children.split(",")

  for (const item in items) {
    const child = document.getElementById("check_thread_" + items[item])
    child.checked = checkBox.checked
    setChecked("check_thread_", items[item])
  }

  updateMarkers();
}

setParents = function (name) {
  const rootChecks = document.getElementsByClassName("checkroot")

  for (const checkBox of rootChecks) {
    const childNames = checkBox.dataset.children.split(",")
    if (childNames.indexOf(name) != -1) {

      var checkCount = 0
      for (const child of childNames) {
        const childCheck = document.getElementById("check_thread_" + child)
        if (childCheck.checked) {
          checkCount++;
        }
      }

      if (checkCount == childNames.length) {
        checkBox.indeterminate = false
        checkBox.checked = true
      } else if (checkCount == 0) {
        checkBox.indeterminate = false
        checkBox.checked = false
      } else {
        checkBox.indeterminate = true
      }

    }
  }
}

changeOrder = function () {
  const checkBox = document.getElementById("order");
  setCustomStyle(checkBox.checked ? "order1" : "order0")

  const issues = document.getElementsByClassName("issueToggle");

  for (const box of issues) {
    if (checkBox.checked) {
      box.classList.add('hidden')
    } else {
      box.classList.remove('hidden')
    }
  }

  const dateMarkers = document.getElementsByClassName("dateMarker");

  for (const box of dateMarkers) {
    if (!checkBox.checked) {
      box.classList.remove('entity_hidden')
      box.classList.add('hidden')
    } else {
      box.classList.remove('hidden')
      box.classList.add('entity_hidden')
    }
  }

}

var markerEvent = null
updateMarkers = function () {

  var scrollMarker = document.querySelector('.scroll-marker');
  scrollMarker.replaceChildren();

  if (markerEvent != null) {
    clearTimeout(markerEvent);
    markerEvent = null;
  }

  markerEvent = setTimeout(() => {
    addScrollMarkers();
  }, 0)

  
}

// from https://stackoverflow.com/a/57634867/1073843
addScrollMarkers = function () {

  markerEvent = null;

  var container = document.querySelector('.container');
  var containerInner = document.querySelector('.box');

  var containerHeight = container.offsetHeight;
  var containerScrollHeight = containerInner.scrollHeight;

  var scrollMarker = document.querySelector('.scroll-marker');
  scrollMarker.replaceChildren();

  var colorfulStuff = document.querySelectorAll('.itemrow_highlight'); // highlighted entries

  colorfulStuff.forEach(function (span) { // loop to create each marker

    var spanTop = span.offsetTop;
    var spanBottom = spanTop + span.offsetHeight;

    var markerTop = Math.ceil(spanTop * containerHeight / containerScrollHeight);
    var markerBottom = Math.ceil(spanBottom * containerHeight / containerScrollHeight);

    if (markerBottom == markerTop) return;

    /*
    if (span.className === "red") { // choose the correct color
        var markerColor = '#f65e5a';
    } else if (span.className === "yellow") {
        var markerColor = '#fec740';
    } else if (span.className === "blue") {
        var markerColor = '#2985d0';
    }*/

    var markerElement = document.createElement("span"); // create the marker, set color and position and put it there
    //markerElement.style.backgroundColor = markerColor;
    markerElement.style.top = markerTop + "px"
    markerElement.style.height = (markerBottom - markerTop) + "px"
    scrollMarker.appendChild(markerElement);

  })
}

toggleMenu = function () {
  const menu = document.getElementById("menu")
  if(menu.classList.contains("menu-collapsed")) {
    menu.classList.remove("menu-collapsed");
  } else {
    menu.classList.add("menu-collapsed");
  }
}