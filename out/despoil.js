
setOrderStyle = function (customStyle) {
  if (document.querySelector('link[id="orderStyle"]')) {
    document.head.removeChild(document.querySelector('link[id="orderStyle"]'))
  }
  const lk = document.createElement('link')
  lk.setAttribute('id', 'customStyle')
  lk.setAttribute('rel', 'stylesheet')
  lk.setAttribute('href', `${customStyle}.css`)
  document.head.appendChild(lk)

  updateMarkers()
}

showAll = function () {
  setTimeout(function () { setAllIssues(true) }, 0)
}

showNone = function () {
  setTimeout(function () { setAllIssues(false) }, 0)
}

setAllIssues = function (enabled) {
  const boxes = document.getElementsByClassName("check_issue")
  for (const box of boxes) {
    box.checked = enabled
    const name = box.id.replace("check_issue_", "")
    setChecked("check_issue_", name, true)
  }

  updateMarkers()
  requestStoreSelection()
}

setChecked = function (prefix, name, noScroll) {
  const checkBox = document.getElementById(prefix + name)
  const boxes = document.getElementsByClassName(name + "_outer")

  if (!noScroll && checkBox.checked) {
    setTimeout(function () { boxes[0].scrollIntoView({ behavior: "smooth" }) }, 10)
  }

  const styleid = 'style_' + name
  for (const box of boxes) {
    if (checkBox.checked) {
      box.classList.add('displayBlock')
      setTimeout(function () { box.classList.add('expanded') }, 0)
      setTimeout(function () { box.classList.add('opacity1') }, 0)


      if (document.getElementById('style_' + name) == null) {
        var style = document.createElement('style')
        style.id = styleid
        style.innerHTML = '.ei_' + name + ' { display: block !important; }'
        document.getElementsByTagName('head')[0].appendChild(style)
      }

    } else {
      box.classList.remove('opacity1')
      setTimeout(function () { box.classList.remove('displayBlock') }, 100)

      var style = document.getElementById('style_' + name)
      if (style != null) style.remove()
    }
  }

  const cousins = document.getElementsByClassName("check_" + name)
  for (const cousin of cousins) {
    if (cousin != checkBox) {
      cousin.checked = checkBox.checked
    }
  }

  setParents(name)
  if (!noScroll) {
    requestStoreSelection()
  }
}

toggleIssue = function (name) {
  const box = document.getElementById("check_issue_" + name)
  box.checked = !box.checked
  setChecked("check_issue_", name)
  updateMarkers()
}

togglePush = function (name) {
  const checkBox = document.getElementById("check_" + name)
  checkBox.checked = !checkBox.checked
  setPushed(name)
}

setPushed = function (name) {
  const checkBox = document.getElementById("check_" + name)
  const boxes = document.getElementsByClassName(name)
  for (const box of boxes) {
    if (box.nodeName == 'DIV') {
      if (checkBox.checked) {
        box.classList.add("event_highlight")
        box.parentElement.classList.add("itemRow_highlight")
      } else {
        box.classList.remove("event_highlight")
        box.parentElement.classList.remove("itemRow_highlight")
      }

    } else if (box.nodeName == 'SPAN') {
      if (checkBox.checked) {
        box.classList.add("entity_active")
      } else {
        box.classList.remove("entity_active")
      }
    }
  }
  updateMarkers()
}

highlightNone = function () {
  setTimeout(function () { 
    const boxes = document.getElementsByClassName("check_entity")
    for (const box of boxes) {
      box.checked = false
      const name = box.id.replace("check_", "")
      setPushed(name)
    }
  }, 0)
}

setChildren = function (name) {
  const checkBox = document.getElementById("check_" + name)
  const items = checkBox.dataset.children.split(",")

  for (const item in items) {
    const child = document.getElementById("check_thread_" + items[item])
    child.checked = checkBox.checked
    setChecked("check_thread_", items[item])
  }

  updateMarkers()
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
          checkCount++
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
  const checkBox = document.getElementById("order")
  setOrderStyle(checkBox.checked ? "order1" : "order0")

  const issues = document.getElementsByClassName("issueToggle")

  for (const box of issues) {
    if (checkBox.checked) {
      box.classList.add('hidden')
    } else {
      box.classList.remove('hidden')
    }
  }

  const dateMarkers = document.getElementsByClassName("dateMarker")

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

  var scrollMarker = document.querySelector('.scroll-marker')
  scrollMarker.replaceChildren()

  if (markerEvent != null) {
    clearTimeout(markerEvent)
    markerEvent = null
  }

  markerEvent = setTimeout(() => {
    addScrollMarkers()
  }, 1000)
}

// from https://stackoverflow.com/a/57634867/1073843
addScrollMarkers = function () {
  markerEvent = null

  var container = document.querySelector('.container')
  var containerInner = document.querySelector('.box')

  var containerHeight = container.offsetHeight
  var containerScrollHeight = containerInner.scrollHeight

  var scrollMarker = document.querySelector('.scroll-marker')
  scrollMarker.replaceChildren()

  var highlights = document.querySelectorAll('.itemRow_highlight')

  highlights.forEach(function (span) { 

    var spanTop = span.offsetTop
    var spanBottom = spanTop + span.offsetHeight

    var markerTop = Math.ceil(spanTop * containerHeight / containerScrollHeight)
    var markerBottom = Math.ceil(spanBottom * containerHeight / containerScrollHeight)

    if (markerBottom == markerTop) return

    var markerElement = document.createElement("span")
    markerElement.style.top = markerTop + "px"
    markerElement.style.height = (markerBottom - markerTop) + "px"
    scrollMarker.appendChild(markerElement)

  })
}

toggleMenu = function () {
  const menu = document.getElementById("menu")
  if(menu.classList.contains("menu-collapsed")) {
    menu.classList.remove("menu-collapsed")
  } else {
    menu.classList.add("menu-collapsed")
  }
}

setCookie = function (c_name, value, exDays) {
  var exDate = new Date();
  exDate.setDate(exDate.getDate() + exDays);
  var c_value = encodeURIComponent(value) + ((exDays == null) ? "" : "; expires=" + exDate.toUTCString());
  document.cookie = c_name + "=" + c_value;
}

getCookie = function (c_name) {
  var i, x, y, cookies = document.cookie.split(";");
  for (i = 0; i < cookies.length; i++) {
      x = cookies[i].substring(0, cookies[i].indexOf("="));
      y = cookies[i].substring(cookies[i].indexOf("=") + 1);
      x = x.replace(/^\s+|\s+$/g, "");
      if (x == c_name) {
          return decodeURIComponent(y);
      }
  }
}

storeSelection = function () {
  var checks = document.getElementsByClassName("check_issue")
  var hashes = []
  for (let check of checks) {
    if (check.checked) {
      hashes.push(check.dataset.hash);
    }
  }
  cookieBody = hashes.join(";")
  setCookie("despoil_checked", cookieBody, 365)
  console.log("Stored selection state")
}

var storeEvent = null
requestStoreSelection = function () {

  if (markerEvent != null) {
    clearTimeout(storeEvent)
    storeEvent = null
  }

  storeEvent = setTimeout(() => {
    storeSelection()
  }, 1000)
}

restoreSelection = function () {
  var hashes = getCookie("despoil_checked").split(";")

  var checks = document.getElementsByClassName("check_issue")
  for (let check of checks) {
    check.checked = hashes.includes(check.dataset.hash)
    setChecked("check_issue_", check.id.replace("check_issue_",""), true)
  }

}

showIntro = function() {
  var cookie = getCookie("despoil_intro")
  if (cookie != null) {
    const introDiv = document.getElementById("intro_" + cookie)
    if (introDiv != null)
      introDiv.classList.add('hidden')
  }
}

hideIntro = function (cookieId) {
  const introDiv = document.getElementById("intro_" + cookieId)
  introDiv.classList.add('hidden')
  setCookie("despoil_intro", cookieId, 365)
  updateMarkers()
}

////////////////////

window.addEventListener('resize', function(event) {
  updateMarkers()
}, true)

showIntro()

setTimeout(function () { restoreSelection() }, 10)