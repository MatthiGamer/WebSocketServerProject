import React, { useState } from "react";

export function TableRow({id, title, content}){

  const [isExpanded, setIsExpanded] = useState(false);

  function switchRow(){
    setIsExpanded(!isExpanded);
  }

  return (
  <tr>
    <th scope="row">
      {id}
    </th>
    <th>
        {title}
    </th>
    <th>
      {isExpanded ? content : content.length > 300 ? `${content.substring(0,300)}...` : content}
      {content.length > 300 ? (
      <button onClick={() => switchRow()} className="btn btn-secondary w-100">
        {isExpanded ? "Collapse" : "Expand"}
      </button>
      ):""}
    </th>
  </tr>
  )
}