// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
class WebButton
{
  public static MouseDown (element: HTMLElement, cssClass: string): boolean
  {
    if (SmartPage_Context && SmartPage_Context.Instance!.IsSubmitting())
      return false;

    element.className += " " + cssClass;
    return false;
  }

  public static MouseUp (element: HTMLElement, cssClass: string): boolean
  {
    element.className = element.className.replace (cssClass, '');
    return false;
  }

  public static MouseOut (element: HTMLElement, cssClass: string): boolean
  {
    element.className = element.className.replace (cssClass, '');
    return false;
  }
}

function WebButton_MouseDown (element: HTMLElement, cssClass: string): boolean
{
  return WebButton.MouseDown(element, cssClass);
}

function WebButton_MouseUp (element: HTMLElement, cssClass: string): boolean
{
  return WebButton.MouseUp(element, cssClass);
}

function WebButton_MouseOut (element: HTMLElement, cssClass: string): boolean
{
  return WebButton.MouseOut(element, cssClass);
}