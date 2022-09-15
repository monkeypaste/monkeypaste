var DefaultEditorWidth = 1200;

var IgnoreNextTextChange = false;
var IgnoreNextSelectionChange = false;

var IsLoaded = false;

var EnvName = "";

var IsPastingTemplate = false;
var IsSubSelectionEnabled = false;

var EditorContainerElement = null;
var QuillEditorElement = null;

var UseBetterTable = true;



function init_test(doText = true) {
	let sample1 = "<html><body><!--StartFragment--><p style='font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17); font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>This awesome!! article can be considered as the fourth instalment in the following sequence of articles:</p><p style='font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17); font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>This article can be considered as the fourth instalment in the following sequence of articles:</p><p style='font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17); font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>This article can be considered as the fourth instalment in the following sequence of articles:</p><ol style='margin: 10px 0px; padding: 0px 0px 0px 40px; border: 0px; color: rgb(17, 17, 17); font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'><li style='margin: 0px; padding: 0px; border: 0px; font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17);'><a href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa'	style='margin: 0px; padding: 0px; border: 0px; text-decoration: none; color: rgb(0, 87, 130);'>Multiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks</a></li><li style='margin: 0px; padding: 0px; border: 0px; font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17);'><a href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A'	style='margin: 0px; padding: 0px; border: 0px; text-decoration: none; color: rgb(0, 87, 130);'>Basics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework</a></li><li style='margin: 0px; padding: 0px; border: 0px; font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17);'><a href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B'	style='margin: 0px; padding: 0px; border: 0px; text-decoration: none; color: rgb(0, 87, 130);'>Multiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples</a></li></ol><p style='font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 15px; line-height: 1.4; color: rgb(17, 17, 17); font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>If you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.</p><!--EndFragment--></body></html>";
	let sample_big = sample1 + sample1 + sample1 + sample1 + sample1 + sample1 + sample1 + sample1 + sample1;
	let sample_buggy_table = '<h2 style="box-sizing: border-box; color: rgb(10, 10, 8); font-weight: 600; line-height: 1.3; margin: 0px; font-size: 26px; font-family: Lato, Roboto, Arial, Tahoma, sans-serif; padding: 0px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; letter-spacing: normal; orphans: 2; text-align: left; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;"><span id="All_Attributes_of_the_anchor_Element" style="box-sizing: border-box;">All Attributes of the<span> </span><a href="https://html.com/tags/a/" style="box-sizing: border-box; background: 0px 0px; color: rgb(18, 18, 18); text-decoration: none; transition: all 0.2s ease-in-out 0s;"><code style="box-sizing: border-box; font-family: monospace, monospace; font-size: 26px; padding: 2px 4px; color: rgb(199, 37, 78); background-color: rgb(249, 242, 244); border-radius: 4px;">anchor</code></a><span> </span>Element</span></h2><table style="box-sizing: border-box; border-collapse: collapse; border-spacing: 0px; border-top: 0px; margin: 0px 0px 1.5em; max-width: 100%; position: relative; table-layout: fixed; width: 2691.22px; z-index: 1; color: rgb(0, 0, 0); font-family: Lato, Roboto, Arial, Tahoma, sans-serif; font-size: 10px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: left; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;"><tbody style="box-sizing: border-box;"><tr style="box-sizing: border-box; border-bottom: 1px solid rgb(240, 240, 240);"><th style="box-sizing: border-box; padding: 12px; background: rgba(0, 0, 0, 0.05); font-weight: 700; border-bottom: 1px solid rgba(0, 0, 0, 0.05); text-align: left; font-family: inherit; font-size: inherit; vertical-align: middle;">Attribute name</th><th style="box-sizing: border-box; padding: 12px; background: rgba(0, 0, 0, 0.05); font-weight: 700; border-bottom: 1px solid rgba(0, 0, 0, 0.05); text-align: left; font-family: inherit; font-size: inherit; vertical-align: middle;">Values</th><th style="box-sizing: border-box; padding: 12px; background: rgba(0, 0, 0, 0.05); font-weight: 700; border-bottom: 1px solid rgba(0, 0, 0, 0.05); text-align: left; font-family: inherit; font-size: inherit; vertical-align: middle;">Notes</th></tr><tr style="box-sizing: border-box; border-bottom: 1px solid rgb(240, 240, 240);"><td style="box-sizing: border-box; padding: 12px; border-bottom: 1px solid rgba(0, 0, 0, 0.05); font-family: inherit; font-size: inherit; vertical-align: middle;"><a href="https://html.com/attributes/a-hreflang/" style="box-sizing: border-box; background: 0px 0px; color: rgb(63, 136, 197); text-decoration: none; transition: all 0.2s ease-in-out 0s;">hreflang</a></td><td style="box-sizing: border-box; padding: 12px; border-bottom: 1px solid rgba(0, 0, 0, 0.05); font-family: inherit; font-size: inherit; vertical-align: middle;"></td><td style="box-sizing: border-box; padding: 12px; border-bottom: 1px solid rgba(0, 0, 0, 0.05); font-family: inherit; font-size: inherit; vertical-align: middle;">Specifies the language of the linked resource.</td></tr><tr style="box-sizing: border-box; border-bottom: 1px solid rgb(240, 240, 240);"><td style="box-sizing: border-box; padding: 12px; border-bottom: 1px solid rgba(0, 0, 0, 0.05); font-family: inherit; font-size: inherit; vertical-align: middle;"><a href="https://html.com/attributes/a-download/" style="box-sizing: border-box; background: 0px 0px; color: rgb(63, 136, 197); text-decoration: none; transition: all 0.2s ease-in-out 0s;">download</a></td><td style="box-sizing: border-box; padding: 12px; border-bottom: 1px solid rgba(0, 0, 0, 0.05); font-family: inherit; font-size: inherit; vertical-align: middle;"></td><td style="box-sizing: border-box; padding: 12px; border-bottom: 1px solid rgba(0, 0, 0, 0.05); font-family: inherit; font-size: inherit; vertical-align: middle;">Directs the browser to download the linked resource rather than opening it.</td></tr><tr style="box-sizing: border-box; border-bottom: 1px solid rgb(240, 240, 240);"><td style="box-sizing: border-box; padding: 12px; border-bottom: 1px solid rgba(0, 0, 0, 0.05); font-family: inherit; font-size: inherit; vertical-align: middle;"><a href="https://html.com/attributes/a-target/" style="box-sizing: border-box; background: 0px 0px; color: rgb(63, 136, 197); text-decoration: none; transition: all 0.2s ease-in-out 0s;">target</a></td><td style="box-sizing: border-box; padding: 12px; border-bottom: 1px solid rgba(0, 0, 0, 0.05); font-family: inherit; font-size: inherit; vertical-align: middle;">_blank<br style="box-sizing: border-box;">_parent<br style="box-sizing: border-box;">_self<br style="box-sizing: border-box;">_top<br style="box-sizing: border-box;">frame name</td><td style="box-sizing: border-box; padding: 12px; border-bottom: 1px solid rgba(0, 0, 0, 0.05); font-family: inherit; font-size: inherit; vertical-align: middle;">Specifies the context in</td></tr></tbody></table><span style="box-sizing: border-box; color: rgb(0, 0, 0); font-family: Lato, Roboto, Arial, Tahoma, sans-serif; font-size: 10px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: left; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;"><br style="box-sizing: border-box;"><br style="box-sizing: border-box;">Read more:<span> </span><a href="https://html.com/attributes/a-target/#ixzz7eVW0Csar" style="box-sizing: border-box; background: 0px 0px; color: rgb(0, 51, 153); text-decoration: none; transition: all 0.2s ease-in-out 0s;">https://html.com/attributes/a-target/#ixzz7eVW0Csar</a></span>';
	let sample_template_single = '<p>YOYOYO whaat up!&gt;!??</p><p> <div class="ql-template-embed-blot" templateguid="b86fcf0b-59ac-48b7-8209-0b849834da00" templateinstanceguid="c58cc3c6-21aa-4f06-9970-d136c25e42dd" templatename="Template #1" templatetype="dynamic" templatecolor="#775F3C" templatedata="" templatetext="undefined" templatedeltaformat="{}" templatehtmlformat="Template #1" style="background-color: #775F3C;color:white;" contenteditable="false" isfocus="false" spellcheck="false">Template #1</div>  </p><p><br></p><p>hkjh</p><p><br></p>';
	let iconBase64_1 = 'iVBORw0KGgoAAAANSUhEUgAAAMgAAADICAYAAACtWK6eAAAABmJLR0QA\/wD\/AP+gvaeTAAAgAElEQVR4nO2dd3wU1drHf2d2NtmQhJACIYSWUKUXXwERBEUUELBcuKD4iqJcvfYueNW8XtsVC+JVr9gQO0VQQLBBLgIqEKQXKUlIIAnphWTbzPP+EYIJbJItM3Nmdue7n\/Nhw+yc85zznN+cMzOnMJiowogFea3dbmsKA6USoxQw1hFEcQAlACwOoDiARZ\/5eTiAFudE4QJQdeZ7DYCS2sBKQFQMRicJ7JjAcMwlWTK3VcadQBqTNcpeyMB4G2B0hswvSGSEvoDQTwD1JaAfgG4Aops7V2EcYMgEYQ\/A9gDyHguwe9P9bTLBGGlsS9BgCsQHxi04HF5GLQczSRhGAg0HMAyEtrztaoZKENsKJm8hsF9ssP6S\/kBsGW+jjIIpkGa45LWCVImx8QAmABgFgo23TQEiA7QdEFbLAr797d6EHWYL0zimQM5hyhKynMg9dTmBjQcwFqAL1EhHLzWSAXkEWgfGfrKFsa\/T72pT1fxZoYMpEAAgYkNeKRwuMJpGDFMAtOFtEieqAVrNGPu8laVi7dp7uzl4G8SbkBbIsFeLkgFpFgGzAHTkZohempOGlAP4TAbe2fpQ4i7exvAi5AQyZQlZjucUXimAZlPtfYUYeKz6rOGKwfAbgd4RXeKyzY+1ruRtjpaEjECG\/6sw2m2RbmUM9wFI8SeOIJeBN5SB4T+yW3h926Nt8nkbowVBL5Ah\/8ptD9FyH8BuBxADwKzpAUKAgzEsBtH83x5O2s\/bHjUJWoGMWJDX2uVkcwi4EzDKo1nDKZcArADwZLAKJegEMmRBcUs4nQ8BeADav83WH9poTmLAJ3Aj7dc5SVmapKgRQSOQwe+QVSzPn00MTzOgdf1jhrsuGxcHI\/zbKjmf2zSnUylvY5QgKATyP\/PyJwqglwjoydsW\/ujiclBMwDNyy6S3M\/7GXLyNCQRDC+SiefkpDPJCAsbwtuUsuqifOoHwByPM+u3xdpt4m+IvxhRIGglDWuTdQ8BzACJ5mxOMKKhzGaC3wyhsjhHfoRhOIENezusly3gPoGG8bQluvJSI90o6TiTcsf3xpLV+GsQFwwhkyhKyZGXmPQpGaQDCzh4wuzTGguEzWbb\/PePxLuW8TfEGQwhk4MsnO4kSLQYwkrctJoHDgGwi9r\/bHm+3kbctzaF7gVz44olpjLH\/ABTD2xZd0EyLqb8GtVGLJBBejHIkp6WnMbeWFvmCbgUy+J3tVlaS9CoY7g4sJv1VGZM\/YcB6t2yZ9vvcpELetnhClwL5n5cy20ISlxBjI3jbYqIJx2USrtsxp10Gb0PORXcCGfx8zhAIbDkDknnbAsBsgLTDToQ7Mua2\/4i3IfXRlUAGv5A7hQGLobvBheqqxNRgPQgvZsxpP1cv8+R1I5ALn895lBheACDwtiWk0UG1ZGBfRjldM9PTUuz8beHMqDQSK8Jy\/s0Y+xsAXTjIRA\/QZoSJ12Q83K6IpxVcBTLs1ZwIpwNLQZjA0w7tMNXvI38IsmXMtn8k5\/AygJtAeqedirKF1XwDsNG8bAgURaq7qZlmYNmyjCt2PtnhMJfUeSQ68Pm81kx2fwdGA3mkb2IsCCixEBu3\/R8dtmqdtuYC6ftCdqxVEn4yxREIBm52\/De9CEQjdzzZ+YCC1jSLpgKpFQfbAEb9tUy3ATqpW4ktLUiIsiApRkS7GBFJMRbERVoAABFhDKLAYLUw1LhkVNplVNTIqLDLyCxyI7vEhexiF6qdOsmMDwRmMRVCYKN+n9tJs\/nvmglkVFqmrUIU1gC4TKs0PcGjSkWFC+jXPgx92oWjT7vaf1u1CPxpdkGFhP15TmzPtmNbth1HT7n0on81yRZE98iMx7sc1yIxTQQyKo3Ecuvx5SBM0iI9PdAxTsTIbhEY0S0CgzrYYNHg7U5ZtYzfsuxYt+80fjlqh1OiJq4IBpYSwxHRTSO2paWovjaXJgIZ8Gz2O4ww+\/wjBnaSB+IjLbi6XyQm9Y9CSoKVqy2Vdhk\/HazG51srcfiUk6stKrFdstlG7X6k7Wk1E1FdIAOezZrLCM+pnQ4vBAYM7xKBawZGYUS3CIgC93evDSAAm47U4MPN5diZ4\/9a1Dq9lK3u1qPTNUunMkmtBFT15qBnsmcQo8VKpKM3B1ktDJMHRGHWxS3RNkaB5X01YMvRGsz7rhRZxUouNMLXMwT8Z9dTKXeqFb9qAhn4TNYwAtJRf3qsKmjrIKuFYXL\/KMy6JMYwwqiPWyZ8vrUS7\/5chkp70Gxp+MDOp1LmqxGxKgLp\/2xOMpPdGQAS1YifBwIDJvWPwuyRrZBkQGGcS3GVhLkrC7E1M8DxgPpo2mUiGrcrLfV7pSNWXCCj0jaIZULnHwCMUjpuj2jgoAuSwjBnfDz6Joern5iGyAQs3FiGhRvLIHOv6AEbkCeKwuCMJzrnKWFNHYoLZEBa1jwwerjub+7l7jXnWxptE3DX6FhMvbAldHbvrShbjtbgkaWncNpp+C7XJrFtyWUZf7tQsZssi1IRAcCA\/8ucTIxehw6G0QfKJd1a4J2b2uKizhFghs9N03SIs2JoagTWH6iG3WWcS5oHOlJVRER++us\/KBWhYq7vnZbZVgTtAZCgVJw8sFoY7r08FjOGxRhf5T6SWeTCnR\/nI79Ct4uMNAMBgEygK3andV2vRIzK1IE0EgYgcwMZfN2qTvFWvHh9G1yQpPKDNx2TV+7G7MV5OF6ikzWn\/WvQTsku1m\/v86kFgSavyACI\/uzoXTJoJMG4n0kDovD57HYhLQ4ASIoR8faMtoixCbWVk3fwjzbMKi\/w++x6BNyC9E872p2AnSBEKGGQ1ogWhicmJOC6QeZeO\/X59VgN\/v5JPiT+j7f8hggz9jzT5dNA4giwBSFGoIVGFUe0TcBbN7Y1xeGBoakReOCKWIDIEIE8BdD8gXMOt24+t40TkED6P3X0bhAuVbRN1ahAk1uJWDyrHYakGlLbmnDTsFYY0yuKey\/Lm+AJBiS4ReGtQMrA7y5W\/ycOJcsWyz7U7RxrIAZ0sGH+9LZnJyiZNE5xlYRr38xBWbVq4wFVh8Cu3vtMlzX+nOt3CyKJlpdhQHGM7N4C797czhSHl8RHWfDg2DjuXahAAiP59a73HPZrGIRfAunz1LErmIxpzbd9\/AunfhjcyYaXpiQi3BpqbzgCY1L\/lujaRp2nexp1wbpExNCj\/tjnu0DSSGAkveB\/z5Afl\/WMRIswc+FGXxEE4I7RccpU1nOuW1ophMAeuWDu\/iSf8+7rCX2kw7MJGKxh3hQL+076P2Eo1BlzQRTaxYjn13BfA78aEG0RxHm+5tunvkbvtH1RzB12BJoOYyfFYkppHYZv7u2kWHyBUOWQsS2zBgdO2pFV5ELpmZtgWSbERlrQKT4MfZJtGNolAhE6afUWbSrFK99xXQk0UGTI8sC9z\/fc7e0JPk1sYG7xHoAMO8cju8iJ0w4ZkeF8KlxFjYw1uyuwbk8VduXYvXoJZ7MyjOwRiRuHtsKgTnwfSY\/vF43XviuCTEpetjRFAIR\/Apjs7QletyCDHzsa47BKxwDE+WSSSiXpb7SLbmuPCztrW9FyS1x4b2MpVu2sgMPtf4GM7hmJp69JREIUvydwNy7Mwa7jvk6y0pecSGBD9\/+z+2\/e\/NbrS6ndKj1ChDifu51QJ\/hO7Zn7T2q3on5+uRtzl+djwvwsLNteBodbRiC53nCwCtcuyML2rBrN8nAuAzvavLI1UG\/5hZfmMIle9jZKrwTS49GD0SC6m+MNlgKhlv0n1L9Rl2TCok2lmDQ\/C9\/sqIBUtz6VAqH0tITZH+bix31VqufDE\/07RPB3ZeA6vKTvnMOXevNDrwQiiuw2EGK4F4ACYd8JdVuQrCInpr19HC+vLUS1SjP0nG7Co0vyuLQk\/drbOLpPuY9bkM\/Oem2KZgXSO21fGICHvInMG3hrJOvMjboarNlVgalvZWP\/SbuizvT0cbhlPPrlSZTXaDsEJDFGRGwLAQBB+5e9UCwwma7uM\/dgv+by26xAZKc4jUDJSjlW8SrvYyHLsvL3IZIMPL\/qFB79Mg+n7bJmai8od+PVddrvnhwbaam1weDIMu5r7jfNCoQR3aeVw\/0KfqDkfchph4y7P87Fp7+UKhanL6zIKMdRjZcWjQ43+ji22srDGE3v9\/CRNk39skmB9JxzYBiAQUqapgeUug8pq5Yw890cbDzk6\/Kwvl0Fzn7o\/OCWCB9sLFYkP94SZWMebTFOqO1QyIQIp+ia1VRemxSIhXBHAwc14iSjhX0nAr+5LTkt4Zb3jmPfiRq\/HeRtaK61XLenUrUHAp4IpvFsDJiNNGo0Q40e6Pv47lgZmEIKdGn0RnaRE1UB3KgXV7kx893jOJSnj7FdNU4ZW49Va5eeSyMxatNN79zTfnBMYyY0KhAJYX8FjDmVtjlkAg74eaNeXi1h5sIcHMl38L\/\/qtfM\/HpE1V0AGlBa5fatCfQ3BFA4vnxAdEtjeW1UIER0uy8OMlrYl+u7QBxuwt0f5+LoKbuiDvL\/82eK\/greH4pPS+ppXqHgI9f2fmCfxyFUHgcr9p6zr5ckU9DdnNfH1\/sQmYA5S05ie6Z2XRlfyCrS7klWSaXbr1qoY8KlMEwB8M65Bzy2ILLErlfdpMbQ6BKzM7sGGVnVyClxeTWA8LV1p7B2VwX\/S2MjobRKmxeGeWUu2F2BjSnTZSB4rPMeWxACpmh7hdD+cpRb4sSMt7PO\/h0fJaJNSxFtY6xIamVFYkzt98QYEUcKHHgvXd\/zIFwSwSURrBZ1pxPvzlF3eAvHaje665zDrY+80K3Bm9fzBNLrsX29ZaK+KpumO4qr3Ciucms62heA4boqu47X1N4\/Bx+ixe36C4C3G\/znub+SZUwOzvwHN+EiU731AIDdxxtrQYKh1tA1OEcg592DEGGcZvYoig76sRxDKw2WMapxytibW9OIDV7Av5iaC5f2e3hXZH2TG7QgfR\/fHeuUaKh3uTUgwXCRa4SUBPUX3U4\/UAm7hzf2QVSs4TVMGA1gdd1\/NBCIQ2JjGfk2T10vBJGT\/KJrovrbw63dXaFBOXP2JGE8GhMIycJYsFCvasbM\/6CUFqrGX+2QsXF\/JYL1Dr0OBlxR\/+8G9yACk8co3rHTwVtz34Ky2dciMAAXpTboOivOT\/srtBuDxZeuqY\/t61j3x9kW5IKH9neSSOro+RwTPdOrXQQSotXtGX+8qUT\/bSud98UvBLc0AsCnQD2BSCQN1XpTPt0XuK8o5CBfmThI3TXEf8+qxs4s7QZDcocwDOcKhEDDg6\/GBj8WgWHCQHUF8uF\/9T2KQGkYo+F138\/egzDgYj7mmATC+AExaNPSqlr8uSVO\/LC3wvNBHdx\/qRJk1rfrPb+2BM60IKPSNognKtDHnwLkStC3eJ4zWPe\/jAG3jw5oh7FmeeO7ArilkLg5r49FECP6A\/hZBIDc8tgeDPDwIL1pB5nw5dKe0ejZzqZa\/AdP2rFyO5\/FKHgjA31RJxAGoT+Z1V7\/1HORRWB45Gqft7vwiXmr8iCHXONRCwP1B850sYioj9dPsEwd6YKpQ+PQPUm91mPLH5XYeLCRew8D4mu1JVZ7y1H7FIuhr7oV31SVkiXQpqUVD45XbxcKl0R4duVJ\/l7jaQDVEwgRdeNoin7hXkPOhzFg3o0d0CpSvReDb6wrwB9az4vRHy1T7tuTKALEQLvMN+gG4S8XxWF492jV4j+Sb8d7G055OKLDq4XaCFInsdP9GW1BokGW9wlBJ9Wjd\/sIPP2XZNXilwmY83kOnDoac8XT44KMFFGULSm6GcGrEzPqoxeT4qJEvHVrCmxW9VY1\/Oi\/hdgRSkNKmoGIUkWAOhKZ+4brGauF4Y2ZnZEcp96kqP0najBvdZ5q8f+JXi45aN4Uho4igRl2U07\/MJCDUPu+49WbOmFotyjVzKh2yrh\/URYcTm33GtE9hDYigeL10sMyaQhjwHPTOmD8wFaqpvPMslwcLVDuqVUQVad4kRESeFuhBsHgpDmTkzFlaLyqaXyzvRRLftVo+wTFnKKNdwmUIBJYfHBUJy9QNJvqltldV7bFbZc1ubdLwOzMOo3HPstGsE+j9RcGIUEE5PhQ0YdRuH5IPB6c0E7VNIor3bjnw0w4dPRIV39QrAiCurP9TXzi8j4xePGGjmAqPlh0S4S73j+GE8UNF7wOvetkszkOEwGo9+zQxxIPdQcNSonEG7emwCKo+9g9bWkOfjtSqWoawYJIROqvOGbSLDEtLHjj1lRVXwQCwPvrC\/DpJu13xtUEFa6w6rYgoUYADnpheie0i1XXFasySvD8V7mh2FT7jQgyBcKbm0a2xriBsaqmkb6vHA98lAnJfGLlE4qMmTZnI\/pPaqINj1\/bXtU09udW4+4PjsEtKeynEHC7SCB9bNXqDwZ3EGPAvBmdVd1WObfYgVvfOoKqGnMYiT+IIGi3uZ1JAyYMisPgVPXGWOWVOTF9\/h\/IL\/PGxQa\/2viIt7kVAbjUNMQ3QsdJYSLDY9eoN7fjVLkLN8w\/hONFIT8zMCBEYuT0ul6GTv1VnZmjEtEhXp0tC4qr3Ljh9UM4puAAxFBFhMQqdTNhKkRoGWHBPePUGUpSWSNhxuuHcDhP3c02G0fBusS\/WtpFxuRi0nrValXRv4OmDktAdITyW6addki4+d+HsD8neGcFaqyZYpEIRXqQqlEItKQYA24YqfwoXaebMOutw9h+tErxuEMWQrEoE4rVHBhn0pDhPVuiS6LyC7498VkWthwKYKE33V0j+RvEgGIRAop1YEsterEDgFrGzBihfOvx\/k\/5+HKzp6V6TAJBJhSLAlG+OfpAG2JaiBjbX9khJZsOlOPZZccVjVNpdFu9mjWM8kVIyDx\/t\/TgQw9OGtErBqJFuf5sRbWEBxepMIQkaAisXBiQKboFMVsgt0IGmZzPn066rI+yO0GlLclCXqlxRwrpHQLLFk4uvLAIoCr+2\/oEa6hFYMCoPsqtTrL5YAWWbjHQvA7ebvAjyIwy61Z3zwIZcIcpX6Hmf6IW\/TpHISFama3SiIB\/fXVc9fxwLC5dIEiUVSsQGX8A+hNIMDlocBflFpz+flcJdmSaU2ZVpjj3\/YtL6uaD7CHgusZ\/G0xVlQ+9Oii3Nsbra05490PTbYGwBzi7gQ7tUqUwTQed5YJkZQRyILcauzPNt+UasAuoE4hL2AuLuT6SWogCQ\/d2yuww4c0LwcavS0F8xVI6a0R7gTMCye085Ghyzi+nAUQqErcfR4KCRrKXmhiBcIVWK\/lmaxHIfLOrOgJoT+2\/AJDGZCLKICIoEdBoQHCHRuis0Nir40V2FHg1O9AkQJyosuwG6i3aQAw\/M8JIfjYFL21ilFk4JkMXI3WpwT\/BAjX8nnFy6fAaoJ5ABJn9SoaZOGUsJyXGKPP+Y8fRCrN7pQEC2Ja672cFYiXHJgesMhAKI7O0RakW5HihOaxEI36u+3JWDFmLRpcBOMjFnCCnTStlWpDCcif\/+yzFQlP3qlwDuS2WX+vKvMHCcYywgRh6NeqhoGrdtctMrEJ7mtudEoLMCQGjfGmwvafeG1pQ91cDz8kkr2VgdymeZhDjjYMUW5CasdCRB6+MEtbV\/7OBQMRqcb07UqoGzD1DACjmJKXegbQIE8wG5DwULhAB3zb8sx65Sy+uYYTN\/PunOgle0XxESgmkdYw1wAwFY1CUqvjTcVvq\/8f5nWNG34JwhdIphzJKCSQ5Tp2F5gCoUNd0kZRvMPy4b2nvBm9izxOIRRaXupn7FfB43BukTnK6lRnnZhUFc79NVThTqDL78twj54kgd\/HFJ4hoK5cnbDBow9wMu7OUeQO+O6sS\/Ls0QRLOr3h2ySqsObfMPbcSjC3hnoEgCm9+mwNXgAsrbD9SgV8OlPHOSvCEcyGsLfrgkvNmoXkUiCThCxBk7pkIkrDjSCUeW3QYkuzJM82TX+rEvQsPwc\/TFUMHRalakIEvPOW50TVoEm\/6bzqAS5stNQ3hXD8CZkj3GDxzQyoGpHo3\/VYmYMmmAjzzxTEUV3qzS4WOS0jHpgGoIgclFS4dfV5fuNEVlCMH3OpGk9NwPaHjUtCBaSeK7fgkPQ87jlaCMYZ2seGwedhdqqTShWVbTuHh9\/\/AR+tPosZh7g6lJgQsLvxi9HJPxxptQXpP2RdWGF6YC6C1apaFOBaBYUBqNHp3jERMCxGlp904mleNbYcrzMXgNESW5QuLPrssw9OxJpf5azMj\/VUAD6hilYm6kMevvp8c9LAdhZ+OGtzY0SZH0TFiHxIj\/wRiOsjECDD6sKnDTb4MLPj00j2Qsd6vqbf1Pr4\/UzAx8RH\/Hl+VUpj1o6aibX4ctoAXQbgsENtDAlPX52CEAmFvenr30eAX3kTT+oYNOwAMVMQmzTCCgxrH2NYbghqLxdW54OMrm1xHyduZPAsITffVTEIMoyuY8FnBZ02LA\/ByQGKhq\/AzAPrapYX3q9dQD5qhSgYkYuxlb1L3bqvV\/UulyD7\/Ww6wyd5mK3gw+qUytPDKW4TFxV9c9r43P\/V6snRhdMwnCZXl\/wCQ6u05oYhqcjJ1qhRuQZCe9\/bHPu0HFj\/9x5tBbJHPJpk0CmPA6L5xaN+6dvXF3ZmV2HnM3NpAWRpcXT4o\/mLMLG\/P9Gm5jWJ3ySfxYvwcEHr4cp4JcG4T0CLcgpsvT8bMMcnoktRwCYC92ZX4ZMNJfP7fPJy2h+g4LHVaTKfELP\/05QSfd5RMmLZ+EkH+2tfzDIdKXRqBMUy7tC3mTO2CpGam0BZVOPHqV1n44IfcoB6bpWHO5pV8OeZRX07wa8vV+Kk\/fg8WXPPWtXBSSmIE3vx7Lwzp4dtehb8frcCdb+7H4ZOnVbIsJCiA6Oxe8un4Cl9O8ksgcdPX94Ys74SPXbRQZkTvWHzycH9ERXj34PBcqh0Sbnt9D77bUaSwZWcI3gaqFmK3lSwd49WTq\/r45a2avR8VRvSekQCGIf6c75EgdtDkoYlY9GA\/tAj3TxxA7YINk4cm4lheNQ7kmC2JTxC2l\/TefBfS032uZX7vap8waVO0bKvZB6CDv3GEAqP6xeGLxwfCavG7qBvgdMv4y3O\/Y\/P+UkXiCwSDXNPcMhOGlH95+Q5\/Tg7Ia3FTf7gSaLhUo5bo3UHJ8TZsmjcUMQqtzVtHfqkDwx\/6FaVV3kzD9YTeS05BGF4oXTJ2rr+nB7T2VcmSK74jsKV+LKCt8k5W+giv3NZTcXEAQNvYcLw0qwcMME6EN5kRNY5nA4kg4MXhLG73fQCVmA5qyORhiRg7KEG1+K+7uC0GdY1RLX7FUWVIVZOBGOGOk6smVgdidsACKfrqqjwCbgk0HtXR0DmiwPDk9K6qZocx4MlpXX22TbkG0v8JdFp8QPRyybKx3wdazoosL1q2dOw3TMYiXTtJw8+Ei1ojta36C+Rf2i8OvTpF+VZxFAs6hmFvTHTYU0pEpdj6u4Kr5l4CHTOdBNwxvpNmac0c097zAQ1bTJ0FB5PlG7MWjbb7WpaeUEwgRd9MrhSAmwDUPlrhX1BcQlJsOIb09O1NeSBMGpro2RbDoLQTMKd0+bjdSlmn6AruJcuu3ALCY6HsoBbh2i6KH2mzQNsOpNIfhUqeAAK+Kls+dr6S5au4N8uWX\/kagZbwLnYuDiLgVJlT04GFBWUO7q2mTsIhi02+BVB2L3NVLnfl0eE3g7BDB4WmXThDRbUbWw+VBVqEXrP05zzN0tIx5SQIE30diOgN6vQHFo22yyRNB1CoSvy65E+1rN3e7FoAiiAT4dP1ueByRdDBi9gzQWKyPLNi+djDapSxah3myhUT\/pCJJgNkD3IHoa4DXBeW\/5ynSTdr3fZCZBfU8M4u7\/Bg2crxK9UqY1XvKCtXjPsFMrv5zKuIYHXQeeFEsR1rt6nfisxbelT1NHTOgooV4xaomYDqj1zKV161hBGeUDsdvfHK8mOQSb1W5KtNedhxuIxD06GbsLpCrHxQtQI+g\/8TFHzAcfDTTeE9bmwL4EIt0tMD+SV2dGwdgX6pLRWPu6TShb8+nxG089Wbv6zQ1nDZPbl6xfWKvAxsCk0EAgCO6d3WhucndwYwQKs0\/UWp6\/6W\/aW4\/pIkxERaFYqx9sb8ttd24fej5YrFqTuacgDRLsFNV5SunqRJASgzi8driEVPXvsOGG7XNl0fUbBn1C+lJb57YSgibcpci\/7vkz\/wyrLm7j3U69rxhBjbIcJxednKazV7jq7xXuiMKhPb3AXQ1zrowzYeFLxl351ZjslPb0VhuROBIMmEuR8ewCvLjniRbhDCcFiWXRO1FEdtsjyYssQS7Yx8D8BMLulzoH3rCCy4sw\/GDPJ9R7vM\/Grc8+YebNxTrIJlUExTakmTCFstNsu4iqVXlaiURKPwEQhQKxJX1PsguhmAIqVrhGvnlYNb4\/7ruuCSPnHN\/raw3ImFa7KxYOUxVHPZyJN\/iTKwbbA4xlWuuE6lq0Nz6XOFWNSkNQsAdncjx7U1R0Mu6BiN6aOScWn\/BPRPbQmLUOuKymo31u8swrdbC\/DV5pOwO2XOlnJlY5hLmFiyVvkhJN7CWSC1RE9a9SiBvQDN74k4Uk\/7raKsaBVV+6Qrr9gOhyukRXEG9mVVZYuZSFdmXoffVvBMvD5Rk1ZPAWExABtvW0IJ9dtov1J48fSqq+cqPTLXH3QjEACInrz6YlnG1wAUXO2AexmbeO8CNwF\/r1498V0VrfEJXQkEAGwTv0mxgK0A0F\/RiE2d6J1CRuyvVWuu3sDbkProTiAAgImrWkQS3gVwA29TTDSA0XYZ7PqaVRP1tc0f9CqQM0RO\/OYBEHsJulgkW19NUJPW6MvU5lhUfTr6Tt43442ha4EAQMS4r4czgX0CoDNvW0wUpYwYu71m9cRlvA1pCt0\/Vq1ZO3mzTRb7E8PHvG0xUYzNMlkG6F0cgAFakPpETPh6GgN7A4o+5VIIr\/nFr7cAAALOSURBVLs1xur\/KIwTxF6sron+J9JHu3kb4w2GEggARE9cleCW5fmMcCNvW\/whhOXxq0Vit1V9N2kfb0N8wXACqaPFhK8nEOE\/ABpZWtBENXxT+WmA\/aMmyv4Glk413AwvwwoEADDu25YR5HoaDHcDCONtjnYYph1aQZLwkP37SZm8DfEXYwvkDOHjv+oukOUVgK7mbYsJAGAPQbjfvnbSet6GBEpQCKSOyKtWXikxvMKA3krGy\/16zd0Ab2GFxOSnHVGuhUbsTnkiqAQCAEhLE2y\/DZwOoqcAdOdtjr5RTHklAF62211vIH1qlVKR6oHgE0gdozaINlvpjQB7CkAqb3OCESKUMYFes8M2HxznbKhJ8AqkjsHvWG0JbaYxRg8TWD+vzjFMl4YXLB+MFtjseLssXds54loT\/AI5C7HwsSuvAqNHADaatzUG5TAjvGx3li1G+i26HDulNCEkkD8Jv+rrHiBpNgE3MyCetz3cabrFdDCw5SBaaP\/h2o16mMSkJSEpkLOM+tAWFhZzPQNuBzACBhibphmE\/RDogzBH2EeV6ROLeJvDi9AWSH0mrI61uZ0TiTAFwFiE1ItHALXtyBYQW8qs8kr7t9dn8zZID5gC8UDk+CVtnS7LZMboaoBdBkDZLWv100mRAPwGYA2IrXD+eN0B3gbpDVMgzTFsSUR4tHUUZPlqYnQxwPpCwzWNVeAYCL9AYGudEtbhJz7rTRkFUyC+MmZJTDgslxDDCAAjQOgLhmjeZjWCE6BDjLFNRPSzRZI31qyfeoK3UUbCFEjAELON+SpFFqgfiPUF0BcMPSAjRUPhOABkAzgMYC8IuwHLHmdpwUFk\/M2lkQ1BiSkQNRm3pLVVtqYwidqTQAmMkEigBAbWhhgSGKElartrNgARDc4l5gKjKgAgsDIGqmAM+USskJFcSAIrlIE8KyyZNRfvPIG0NHO1ORX4f6pfeZwZVBjsAAAAAElFTkSuQmCC';

	let sample_file_list_obj = {
		fileItems: [
			{
				filePath: 'this is test path 1',
				fileIconBase64: iconBase64_1
			},
			{
				filePath: 'this is test path 2',
				fileIconBase64: iconBase64_1
			}]
	};
	let sample_file_list = toBase64FromJsonObj(sample_file_list_obj);
	// reqMsg = {
	//   envName: "web",
	//   isPasteRequest: false,
	//   isReadOnlyEnabled: false,
	//   itemData: sample1 + sample2,
	//   usedTextTemplates: []
	// };


	initMsg = {
		envName: 'wpf',
		copyItemId: 0,
		copyItemType: doText ? 'Text' : 'FileList',
		isReadOnlyEnabled: true,
		usedTextTemplates: {},
		isPasteRequest: false,
		itemData: doText ? sample_template_single : sample_file_list,

		useBetterTable: true
	}

	init(initMsg);
	//disableReadOnly();

	//enableFancyTextSelection();
	//enableSubSelection();
}

function init(initMsg) {
	if (initMsg == null) {
		initMsg = {
			envName: 'wpf',
			copyItemId: 0,
			isReadOnlyEnabled: true,
			usedTextTemplates: {},
			isPasteRequest: false,
			itemData: '',
			useBetterTable: false
		}
	}
	EnvName = initMsg.envName;

	// NOTE table flag is just for debugging
	UseBetterTable = initMsg.useBetterTable;
	if (UseBetterTable == null) {
		UseBetterTable = true;
	}

	CopyItemId = initMsg.copyItemId;
	CopyItemType = initMsg.copyItemType;

	if (CopyItemType.includes('.')) {
		log('hey item type is ' + CopyItemType);
		CopyItemType = CopyItemType.split('.')[1];
		log('now item type is ' + CopyItemType);
	}

	if (!IsLoaded) {
		loadQuill(EnvName, UseBetterTable);
	}

	showEditor();
	initContent(initMsg.itemData);

	if (!IsLoaded) {
		initTemplates(initMsg.usedTextTemplates, initMsg.isPasteRequest);
	}

	initDragDrop();

	initClipboard();

	if (EnvName == "web") {
		//for testing in browser
		document.getElementsByClassName("ql-toolbar")[0].classList.add("env-web");
	} else {
		document.getElementsByClassName("ql-toolbar")[0].classList.add("env-wpf");

		if (initMsg.isReadOnlyEnabled) {
			enableReadOnly();
		} else {
			showEditorToolbar();
			disableReadOnly();
		}
	}

	window.addEventListener("resize",
		function (event) {
			onWindowResize(event);
		},
		true
	);

	updateAllSizeAndPositions();
	window.onscroll = onWindowScroll;

	IsLoaded = true;

	// initial load content length ntf
	//onContentLengthChanged_ntf();

	log('Editor loaded');
}


function loadQuill(envName, useBetterTable = true) {
	Quill.register("modules/htmlEditButton", htmlEditButton);
	if (useBetterTable) {
		Quill.register({ "modules/better-table": quillBetterTable }, true);
	}
	

	registerTemplateSpan();

	// Append the CSS stylesheet to the page
	var node = document.createElement("style");
	node.innerHTML = registerFontStyles(envName);
	document.body.appendChild(node);

	// create quill options
	let quillOptions = {
		//debug: true,
		placeholder: "",
		theme: "snow",
		modules: {
			table: false,
			toolbar: registerToolbar(envName),
			htmlEditButton: {
				syntax: true
			}
		}
	}
	if (useBetterTable) {
		quillOptions.modules['better-table'] = {
			operationMenu: {
				items: {
					unmergeCells: {
						text: "Unmerge cells"
					}
				},
				color: {
					colors: ["green", "red", "yellow", "blue", "white"],
					text: "Background Colors:"
				}
			}
		};
		quillOptions.modules.keyboard = {
			bindings: quillBetterTable.keyboardBindings
		};
	}

	quill = new Quill("#editor", quillOptions);

	quill.root.setAttribute("spellcheck", "false");

	initTableToolbarButton();

	window.addEventListener("click", onWindowClick);

	quill.on("selection-change", onEditorSelectionChanged);

	quill.on("text-change", onEditorTextChanged);

	document.onselectionchange = onDocumentSelectionChange;
	//window.ondblclick = onDocumentDoubleClick;

	getEditorContainerElement().firstChild.id = 'quill-editor';
}

function registerToolbar(envName) {
	let sizes = registerFontSizes();
	let fonts = registerFontFamilys(envName);

	var toolbar = {
		container: [
			//[{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
			[{ size: sizes }], // font sizes
			[{ font: fonts.whitelist }],
			["bold", "italic", "underline", "strike"], // toggled buttons
			["blockquote", "code-block"],

			// [{ 'header': 1 }, { 'header': 2 }],               // custom button values
			[{ list: "ordered" }, { list: "bullet" }, { list: "check" }],
			[{ script: "sub" }, { script: "super" }], // superscript/subscript
			[{ indent: "-1" }, { indent: "+1" }], // outdent/indent
			[{ direction: "rtl" }], // text direction

			// [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
			["link", "image", "video", "formula"],
			[{ color: [] }, { background: [] }], // dropdown with defaults from theme
			[{ align: [] }],
			// ['clean'],                                         // remove formatting button
			// ['templatebutton'],
			[{ "Table-Input": registerTables() }]
		],
		handlers: {
			"Table-Input": () => {
				return;
			}
		}
	};

	return toolbar;
}

function focusEditor() {
	document.getElementById("editor").focus();
}
function hideScrollbars() {
	//document.querySelector('body').style.overflow = 'hidden';
	document.getElementById("editor").style.overflow = "hidden";
}

function showScrollbars() {
	//document.querySelector('body').style.overflow = 'scroll';
	document.getElementById("editor").style.overflow = "auto";
}

function disableTextWrapping() {
	getEditorElement().style.whiteSpace = 'nowrap';
	getEditorElement().style.width = Number.MAX_SAFE_INTEGER + 'px';
}

function enableTextWrapping() {
	getEditorElement().style.whiteSpace = '';
	getEditorElement().style.width = '';
}


function getTotalHeight() {
	var totalHeight =
		getEditorToolbarHeight() + getEditorHeight() + getTemplateToolbarHeight();
	return totalHeight;
}

function updateAllSizeAndPositions() {

	$(".ql-toolbar").css("top", 0);

	if (isEditorToolbarVisible()) {
		$("#editor").css("top", $(".ql-toolbar").outerHeight());
	} else {
		$("#editor").css("top", 0);
	}

	let wh = window.visualViewport.height;
	let eth = getEditorToolbarHeight();
	let tth = getTemplateToolbarHeight();

	$("#editor").css("height", wh - eth - tth);

	updateEditTemplateToolbarPosition();
	updatePasteTemplateToolbarPosition();

	drawOverlay();

	if (EnvName == "android") {
		//var viewportBottom = window.scrollY + window.innerHeight;
		//let tbh = $(".ql-toolbar").outerHeight();
		//if (y <= 0) {
		//    //keyboard is not visible
		//    $(".ql-toolbar").css("top", y);
		//    $("#editor").css("top", y + tbh);
		//} else {
		//    $(".ql-toolbar").css("top", y - tbh);
		//    $("#editor").css("top", 0);
		//}
		//$("#editor").css("bottom", viewportBottom - tbh);
	}


	////$(".ql-toolbar").css("position", "fixed");
	//let toolbarElm = getEditorToolbarElement();
	//let editorContainerElm = getEditorContainerElement();
	//let editorElm = getEditorElement();

	//toolbarElm.style.top = 0;

	//if (isEditorToolbarVisible()) {
	//	toolbarElm.style.top = parseFloat(toolbarElm.style.height);
	//	//$("#editor").css("top", $(".ql-toolbar").outerHeight()); 
	//} else {
	//	editorContainerElm.style.top = 0;
	//	//$("#editor").css("top", 0);
	//}

	//let wh = parseFloat(window.visualViewport.height);
	//let eth = getEditorToolbarHeight();
	//let tth = getTemplateToolbarHeight();

	////$("#editor").css("height", wh - eth - tth);
	//editorContainerElm.style.height = (wh - eth - tth) + 'px';

	//editorContainerElm.style.width = getContentWidth() + 'px';
	//editorElm.style.height = getContentHeight() + 'px';

	//updateEditTemplateToolbarPosition();
	//updatePasteTemplateToolbarPosition();

	//drawOverlay();

	//if (EnvName == "android") {
	//	//var viewportBottom = window.scrollY + window.innerHeight;
	//	//let tbh = $(".ql-toolbar").outerHeight();
	//	//if (y <= 0) {
	//	//    //keyboard is not visible
	//	//    $(".ql-toolbar").css("top", y);
	//	//    $("#editor").css("top", y + tbh);
	//	//} else {
	//	//    $(".ql-toolbar").css("top", y - tbh);
	//	//    $("#editor").css("top", 0);
	//	//}
	//	//$("#editor").css("bottom", viewportBottom - tbh);
	//}
}

function onWindowClick(e) {
	if (
		e.path.find(
			(x) => x.classList && x.classList.contains("edit-template-toolbar")
		) != null ||
		e.path.find(
			(x) => x.classList && x.classList.contains("paste-template-toolbar")
		) != null ||
		e.path.find(
			(x) => x.classList && x.classList.contains("context-menu-option")
		) != null ||
		e.path.find((x) => x.classList && x.classList.contains("ql-toolbar")) !=
		null
	) {
		//ignore clicks within template toolbars
		return;
	}
	if (
		e.path.find(
			(x) => x.classList && x.classList.contains("ql-template-embed-blot")
		) == null
	) {
		// unfocus templates 
		hideAllTemplateContextMenus();
		hideEditTemplateToolbar();
		hidePasteTemplateToolbar();
		clearTemplateFocus();
	}

	let sel = getSelection();
	if (sel.length > 0) {
		// there's a sel range

		let mp = getEditorMousePos(e);
		let editor_rect = getEditorRect();
		if (!isPointInRect(mp)) {
			// ignore when mouse up outside editor
			return;
		}
		let was_sel_click = isPointInRange(mp, sel);
		if (!was_sel_click) {
			// this a workaround for weird click bug to clear selection

			// when click not on selection clear selection and move caret to mp doc idx
			let mp_doc_idx = getDocIdxFromPoint(mp);
			if (mp_doc_idx < 0) {
				// fallback to old range start
				mp_doc_idx = sel.index;
			}
			setEditorSelection(mp_doc_idx, 0);
		}
	}
}

function onWindowScroll(e) {
	if (isReadOnly()) {
		return;
	}

	updateAllSizeAndPositions();
}

function onWindowResize(e) {
	updateAllSizeAndPositions();
	drawOverlay();
}

function onDocumentSelectionChange(e) {
	let range = getSelection();
	if (range) {
		//log("idx " + range.index + ' length "' + range.length);
		drawOverlay();
	} else {
		log('selection outside editor');
	}
}

function onDocumentDoubleClick(e) {
	enableSubSelection();
}

function onEditorSelectionChanged(range, oldRange, source) {
	//LastSelectedHtml = SelectedHtml;
	//SelectedHtml = getSelectedHtml();
	//log("User cursor is at " + (range == null ? -1 : range.index) + ' before "' + (oldRange == null ? - 1 : oldRange.index));
	
	drawOverlay();

	if (IgnoreNextSelectionChange) {
		IgnoreNextSelectionChange = false;
		return;
	}

	if (IsDragCancel) {
		IsDragCancel = false;
		if (oldRange) {
			setEditorSelection(oldRange.index, oldRange.length);
		}
		return;
	}

	if (range) {
		refreshFontSizePicker();
		refreshFontFamilyPicker();

		//if (range.length == 0) {
		//	var text = getText({ index: range.index, length: 1 });
		//	let ls = getLineStartDocIdx(range.index);
		//	let le = getLineEndDocIdx(range.index);
		//	log("User cursor is at " + range.index + ' idx before "' + text + '" line start: '+ls+' line end: '+le);
		//} else {
		//	var text = getText(range);
		//	log("User cursor is at " + range.index + " with length " + range.length + ' and selected text "' + text + '"');
		//}

		//updateTemplatesAfterSelectionChange(range, oldRange, source);
		//coereceSelectionWithTemplatePadding(range, oldRange, source);

		onEditorSelectionChanged_ntf(range);
	} else {
		log("Cursor not in the editor");
	}
	if (!range && !isEditTemplateTextAreaFocused()) {
		if (oldRange) {
			//blur occured
			setEditorSelection(oldRange.index, oldRange.length);
		} else {
			return;
		}
		
	}
}

function onEditorTextChanged(delta, oldDelta, source) {
	updateAllSizeAndPositions();

	if (!IsLoaded) {
		return;
	}
	if (IgnoreNextTextChange) {
		IgnoreNextTextChange = false;
		return;
	}
	let srange = quill.getSelection();
	if (!srange) {
		return;
	}

	updateTemplatesAfterTextChanged(delta, oldDelta, source);

	onContentLengthChanged_ntf();
}

function selectAll() {
	setEditorSelection(0, getDocLength(),'api');
}

function isAllSelected() {
	// NOTE doc length is never 0 and there's always an extra unselectable \n character at end so minus 1 for length to check here
	let doc_len = getDocLength() - 1;
	let sel = getSelection();
	let result = sel.index == 0 && sel.length == doc_len;
	return result;
}


function setHtml(html) {
	//quill.root.innerHTML = html;
	document.getElementsByClassName("ql-editor")[0].innerHTML = html;
}

function setHtmlFromBase64(base64Html) {
	console.log("base64: " + base64Html);
	let html = atob(base64Html);
	console.log("html: " + html);

	quill.root.innerHTML = html;

	var output = getHtmlBase64();
	return output;
}

function setContents(jsonStr) {
	quill.setContents(JSON.parse(jsonStr));
}

function getText(rangeObj) {
	if (!quill || !quill.root) {
		return '';
	}

	let wasReadOnly = isReadOnly();
	if (wasReadOnly) {
		getEditorElement().setAttribute('contenteditable', true);
		quill.update();
	}

	rangeObj = rangeObj == null ? { index: 0, length: quill.getLength() } : rangeObj;

	let text = quill.getText(rangeObj.index, rangeObj.length);
	if (wasReadOnly) {
		getEditorElement().setAttribute('contenteditable', false);
		quill.update();
	}
	return text;
}

function setTextInRange(range, text) {
	quill.deleteText(range.index, range.length);
	quill.insertText(range.index, text);

	//quill.setText(text + "\n");
}

function insertText(docIdx, text) {
	quill.insertText(docIdx, text);
}

function getSelectedText() {
	var selection = quill.getSelection();
	return quill.getText(selection.index, selection.length);
}

function getHtml(rangeObj) {
	let wasReadOnly = isReadOnly();
	if (wasReadOnly) {
		getEditorElement().setAttribute('contenteditable', true);
		quill.update();
	}

	rangeObj = rangeObj == null ? { index: 0, length: quill.getLength() } : rangeObj;

	let text = quill.getText(rangeObj.index, rangeObj.length);
	if (wasReadOnly) {
		getEditorElement().setAttribute('contenteditable', false);
		quill.update();
	}

	if (quill && quill.root) {
		//var val = document.getElementsByClassName("ql-editor")[0].innerHTML;
		clearTemplateFocus();
		var val = quill.root.innerHTML;
		//log('getHtml response');
		//log(val);
		//setComOutput(JSON.stringify(val));

		return val;
	}
	setComOutput('');
	return '';
}

function insertHtml(docIdx, data) {
	quill.clipboard.dangerouslyPasteHTML(docIdx, data);
}

function insertContent(docIdx, data, forcePlainText = false) {
	// TODO need to determine data type here
	if (forcePlainText) {
		insertText(docIdx, data);
		return;
	}
	insertHtml(docIdx, data);
}


function getSelectedHtml(maxLength) {
	maxLength = maxLength == null ? Number.MAX_SAFE_INTEGER : maxLength;

	var selection = quill.getSelection();
	if (selection == null) {
		return "";
	}
	if (!selection.hasOwnProperty("length") || selection.length == 0) {
		selection.length = 1;
	}
	if (selection.length > maxLength) {
		selection.length = maxLength;
	}
	var selectedContent = quill.getContents(selection.index, selection.length);
	var tempContainer = document.createElement("div");
	var tempQuill = new Quill(tempContainer);

	tempQuill.setContents(selectedContent);
	let result = tempContainer.querySelector(".ql-editor").innerHTML;
	tempContainer.remove();
	return result;
}

function getSelectedHtml2() {
	var selection = window.getSelection();
	if (selection.rangeCount > 0) {
		var range = selection.getRangeAt(0);
		var docFrag = range.cloneContents();

		let docFragStr = domSerializer.serializeToString(docFrag);

		const xmlnAttribute = ' xmlns="http://www.w3.org/1999/xhtml"';
		const regEx = new RegExp(xmlnAttribute, "g");
		docFragStr = docFragStr.replace(regEx, "");
		return docFragStr;
	}
	return "";
}

function createLink() {
	var range = quill.getSelection(true);
	if (range) {
		var text = getText(range);
		quill.deleteText(range.index, range.length);
		var ts =
			'<a class="square_btn" href="https://www.google.com">' + text + "</a>";
		insertHtml(range.index, ts);

		log("text:\n" + getText());
		console.table("\nhtml:\n" + getHtml());
	}
}

function isReadOnly() {
	var isEditable = parseBool(getEditorElement().getAttribute('contenteditable'));
	return !isEditable;
}

function enableReadOnly() {
	//deleteJsComAdapter();

	getEditorElement().setAttribute('contenteditable', false);
	

	quill.update();

	hideEditorToolbar();

	scrollToHome();
	hideScrollbars();

	disableSubSelection();
	drawOverlay();
}

function disableReadOnly(isSilent) {
	enableSubSelection();

	if (!isSilent) {
		showEditorToolbar();
		showScrollbars();
	}

	getEditorElement().setAttribute('contenteditable', true);
	//getEditorElement().style.caretColor = 'black';
	//$(".ql-editor").attr("contenteditable", true);
	//$(".ql-editor").css("caret-color", "black");


	//document.body.style.height = disableReadOnlyMsg.editorHeight;
	//$('.ql-editor').css('min-width', getEditorToolbarWidth());
	//$('.ql-editor').css('min-height', disableReadOnlyMsg.editorHeight);
	//document.getElementById('editor').style.minHeight = disableReadOnlyMsg.editorHeight - getEditorToolbarHeight() + 'px';
	//$('.ql-editor').css('width', DefaultEditorWidth);
	//document.body.style.minHeight = disableReadOnlyMsg.editorHeight;

	updateAllSizeAndPositions();

	refreshFontSizePicker();
	refreshFontFamilyPicker();

	drawOverlay();
}

function enableSubSelection() {
	//if (IsSubSelectionEnabled) {
	//	return;
	//}

	//getEditorContainerElement().classList.remove('noselect');
	getEditorContainerElement().style.cursor = 'text';

	IsSubSelectionEnabled = true;

	//onSubSelectionEnabledChanged_ntf(IsSubSelectionEnabled);
	if (!isEditorToolbarVisible()) {
		//getEditorElement().style.caretColor = 'red';
	} else {
		// this SHOULD be set already in disableReadOnly but setting here to ensure state
		//getEditorElement().style.caretColor = 'black';
	}
	showScrollbars();
	drawOverlay();
}

function disableSubSelection() {
	//if (!IsSubSelectionEnabled) {
	//	return;
	//}
	//getEditorContainerElement().classList.add('noselect');
	getEditorContainerElement().style.cursor = 'default';

	IsSubSelectionEnabled = false;
	//onSubSelectionEnabledChanged_ntf(IsSubSelectionEnabled);

	if (!isEditorToolbarVisible()) {
		//getEditorElement().style.caretColor = 'transparent';

		let selection = quill.getSelection();
		if (selection) {
			setEditorSelection(selection.index, 0);
		}
		hideScrollbars();
	}

	drawOverlay();
}

function getSelection() {
	let selection = quill.getSelection();
	return selection;
}

function setEditorSelection(doc_idx,length, source = 'user') {
	quill.setSelection(doc_idx, length, source);
}


function isShowingEditorToolbar() {
	$(".ql-toolbar").css("display") != "none";
}

function hideEditorAndAllToolbars() {
	hideEditorToolbar();
	hideEditor();
	hideEditTemplateToolbar();
	hidePasteTemplateToolbar();
}

function showEditor() {
	getEditorContainerElement().classList.remove('hidden');
}

function hideEditor() {
	getEditorContainerElement().classList.add('hidden');
}

function isEditorHidden() {
	let isHidden = getEditorContainerElement().classList.contains('hidden');
	return isHidden;
}

function hideEditorToolbar() {
	if (EnvName == "web") {
		document
			.getElementsByClassName("ql-toolbar")[0]
			.classList.remove("ql-toolbar-env-web");
	} else {
		document
			.getElementsByClassName("ql-toolbar")[0]
			.classList.remove("ql-toolbar-env-wpf");
	}
	document.getElementsByClassName("ql-toolbar")[0].classList.add("hidden");

	//document.getElementById('editor').previousSibling.style.display = 'none';
	updateAllSizeAndPositions();
}

function showEditorToolbar() {
	document.getElementsByClassName("ql-toolbar")[0].classList.remove("hidden");
	if (EnvName == "web") {
		document
			.getElementsByClassName("ql-toolbar")[0]
			.classList.add("ql-toolbar-env-web");
	} else {
		document
			.getElementsByClassName("ql-toolbar")[0]
			.classList.add("ql-toolbar-env-wpf");
	}
	updateAllSizeAndPositions();
}

function isEditorToolbarVisible() {
	return !document.getElementsByClassName("ql-toolbar")[0].classList.contains('hidden');
}

function getEditorWidth() {
	var editorRect = document.getElementById("editor").getBoundingClientRect();
	//var editorHeight = parseInt($('.ql-editor').wi());
	return editorRect.width;
}

function getEditorHeight() {
	var editorRect = document.getElementById("editor").getBoundingClientRect();
	//var editorHeight = parseInt($('.ql-editor').outerHeight());
	return editorRect.height;
}




function getEditorToolbarWidth() {
	if (isReadOnly()) {
		return 0;
	}
	return document
		.getElementsByClassName("ql-toolbar")[0]
		.getBoundingClientRect().width;
}

function getEditorToolbarHeight() {
	if (isReadOnly()) {
		return 0;
	}
	var toolbarHeight = parseInt($(".ql-toolbar").outerHeight());
	return toolbarHeight;
}

function scrollToHome() {
	document.getElementById("editor").scrollTop = 0;
}

function getEditorContainerElement() {
	if (EditorContainerElement == null) {
		EditorContainerElement = document.getElementById("editor");
	}
	return EditorContainerElement;
}

function getEditorElement() {
	if (QuillEditorElement == null) {
		QuillEditorElement = getEditorContainerElement().firstChild;
	}
	return QuillEditorElement;
}

function getEditorToolbarElement() {
	return document.getElementsByClassName('ql-toolbar')[0]
}

function getEditorRect(clean = true) {
	//return { left: 0, top: 0, right: window.outerWidth, bottom: window.outerHeight, width: window.outerWidth, height: window.outerHeight };
	let temp = getEditorContainerElement().getBoundingClientRect();
	temp = cleanRect(temp);
	//   if (clean) {
	//       temp.right = temp.width;
	//       temp.bottom = temp.height;
	//       temp.left = 0;
	//       temp.top = 0;
	//       temp = cleanRect(temp);
	//}
	return temp;
}

function isEditorElement(elm) {
	if (elm instanceof HTMLElement) {
		return elm.classList.contains('ql-editor');
	}
	return false;
}

async function getContentImageBase64Async() {
	let base64Str = await getBase64ScreenshotOfElementAsync(getEditorElement());

	return base64Str;
}

function getContentImageBase64() {
	let base64Str = getBase64ScreenshotOfElement(getEditorElement());

	return base64Str;
}

function isRunningInHost() {
	return typeof notifyException === 'function';
}

