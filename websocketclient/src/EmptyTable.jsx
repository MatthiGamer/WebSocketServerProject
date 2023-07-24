import React from "react";

export function EmptyTable(){
    return (
        <>
            <tr>
                <th className='text-center'>
                    0
                </th>
                <th className='text-center'>
                -
                </th>
                <th>
                No Data available.
                </th>
            </tr>
        </>
    )
}