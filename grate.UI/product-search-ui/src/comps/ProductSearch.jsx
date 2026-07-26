import { useState } from "react";
import axios from "axios";

export default function ProductSearch() {
    const [searchText, setSearchText] = useState("");
    const [results, setResults] = useState([]);

    async function searchProducts() {
        if (!searchText.trim()) return;

        try {
            const res = await axios.get(
                "http://localhost:5050/api/product/searchoptimA",
                {
                    params: {
                        search: searchText,
                        pageNo: 1,
                        pageSize: 20
                    }
                }
            );

            setResults(res.data.items);
        } catch (err) {
            console.log(err);
        }
    }

    return (
        <div className="container">
            <h1>Product Search</h1>

            <input
                type="text"
                placeholder="Search..."
                value={searchText}
                onChange={(e) => setSearchText(e.target.value)}
            />

            <button onClick={searchProducts}>
                Search
            </button>

            <div>
                {results.map(item => (
                    <div key={item.id} className="card">
                        <h3>{item.name}</h3>
                        <p>{item.description}</p>
                        <p>₹ {item.price}</p>
                        <p>Stock: {item.stockQuantity}</p>
                    </div>
                ))}
            </div>
        </div>
    );
}