import React, { useState } from 'react';
import { TableRow } from './TableRow';
import { EmptyTable } from './EmptyTable';

export function DataTable({webSocket}){

  const [tableRows, setTableRows] = useState([]);
  const [postIDs, setPostIDs] = useState([]);

  function AddTableRow(id, title, content){
    setTableRows(() => {
      if (postIDs.includes(id) == false){
        setPostIDs([...postIDs, id]);
        console.log("Adding new blog entry");
        return [...tableRows, {id, title, content}];
      }
      
      console.log(`Updating blog entry with id ${id}`);
      return [...tableRows.filter(row => row.id !== id), {id, title, content}];
    });
  }

  webSocket.onmessage = (event) => {
    const serverData = JSON.parse(event.data);
    AddTableRow(serverData.id, serverData.title, serverData.content);
    webSocket.send(`Data with id ${serverData.id} received.`);
  };

  return (
    <div className="table-responsible mt-5">
      <table className="table table-bordered border-dark table-striped">
        <thead className='thead-dark'>
          <tr>
            <th className='text-center'>ID</th>
            <th className='text-center'>Titel</th>
            <th className='text-center'>Word Count Map</th>
          </tr>
        </thead>
        <tbody>
          {tableRows.length === 0 ? 
            <EmptyTable/>
            :
            <>
              {tableRows.map((tableRow) => {
                return (
                  <TableRow {...tableRow} key={tableRow.id}/>
                )
              })}
            </>
          }
        </tbody>
      </table>
    </div>
  )
}