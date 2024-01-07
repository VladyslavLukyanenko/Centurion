import {animate, state, style, transition, trigger} from "@angular/animations";

export const slideToggle = trigger("slideToggle", [
  state("visible", style({
    // display: "block",
    opacity: 1,
    height: "*"
  })),
  state("collapsed", style({
    // display: "none",
    opacity: 0,
    height: 0
  })),
  transition("collapsed <=> visible", animate("300ms ease-in-out")),
]);
