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

setChecked = function (prefix, name) {
  const checkBox = document.getElementById(prefix + name);
  //console.log(checkBox, checkBox.checked)
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

setPushed = function (name) {
  const checkBox = document.getElementById("check_" + name);
  //console.log(checkBox, checkBox.checked)
  const boxes = document.getElementsByClassName(name);
  for (const box of boxes) {
    if (checkBox.checked) {
      box.style.margin = 20
      box.style.marginLeft = 400;
    } else {
      box.style.margin = 2
      box.style.marginLeft = 200;
    }
  }
}

setChildren = function (name) {
  const checkBox = document.getElementById("check_" + name);
  console.log(checkBox, checkBox.checked)
  console.log(checkBox.dataset.children)
  const items = checkBox.dataset.children.split(",")
  console.log(items)

  for (const item in items) {
    const child = document.getElementById("check_thread_" + items[item])
    child.checked = checkBox.checked
    setChecked("check_thread_", items[item])
  }
}

setParents = function (name) {
  const rootChecks = document.getElementsByClassName("checkroot")
  console.log(rootChecks)

  for (const checkBox of rootChecks) {
    //const checkBox = rootChecks[checkIdx]
    const childNames = checkBox.dataset.children.split(",")
    if (childNames.indexOf(name) != -1) {
      console.log("==>", checkBox, name)

      var checkCount = 0
      for (const child of childNames) {
        console.log(child)
        const childCheck = document.getElementById("check_thread_" + child)
        if (childCheck.checked) {
          checkCount++;
        }
      }
      console.log(checkCount, "children checked")

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