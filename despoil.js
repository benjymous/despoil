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

showAll = function() {
  setAllIssues(true)
}

showNone = function() {
  setAllIssues(false)
}

setAllIssues = function(enabled) {
  const boxes = document.getElementsByClassName("check_issue")
  for (const box of boxes)
  {
    box.checked = enabled
    const name = box.id.replace("check_issue_","");
    setChecked("check_issue_",name);
  }
}

setChecked = function (prefix, name) {
  const checkBox = document.getElementById(prefix + name);
  const boxes = document.getElementsByClassName(name);
  for (const box of boxes) {
    if (checkBox.checked) {
      box.style.display = 'block';
      setTimeout(function(){ box.style.opacity = 1; }, 0);
    } else {
      box.style.opacity = 0
      setTimeout(function(){ box.style.display = 'none'; }, 260);
    }
  }

  var cousins = document.getElementsByClassName("check_"+name)
  for (const cousin of cousins) {
    if (cousin != checkBox) {
      cousin.checked = checkBox.checked;
    }
  }

  setParents(name)
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
        box.style.margin = 5
        box.style.marginLeft = 250;
        box.style.borderWidth = 5;
        box.style.borderRadius = 15;
        box.style.boxShadow = "rgb(170 170 170) 5px 5px 10px 0px";
      } else {
        box.style.margin = 2
        box.style.marginLeft = 200;
        box.style.borderWidth = 2;
        box.style.borderRadius = 10;
        box.style.boxShadow = null;
      }
    } else if (box.nodeName == 'SPAN') {
      if (checkBox.checked) {
        box.style.background = "black"
        box.style.color = "white"
      } else {
        box.style = null;
      }
    }
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
  setCustomStyle( checkBox.checked ? "order1" : "order0")
}